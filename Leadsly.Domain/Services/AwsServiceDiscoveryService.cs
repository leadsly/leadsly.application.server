using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
using Leadsly.Application.Model.Aws.ServiceDiscovery;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class AwsServiceDiscoveryService : IAwsServiceDiscoveryService
    {
        public AwsServiceDiscoveryService(AmazonServiceDiscoveryClient awsServiceDiscoveryClient, ILogger<AwsServiceDiscoveryService> logger)
        {
            _logger = logger;
            _awsServiceDiscoveryClient = awsServiceDiscoveryClient;
        }

        private string ServiceDiscoveryId { get; set; } = string.Empty;
        private readonly int DefaultTimeToWait_InSeconds = 120;
        private readonly ILogger<AwsServiceDiscoveryService> _logger;
        private readonly AmazonServiceDiscoveryClient _awsServiceDiscoveryClient;

        public async Task<CreateServiceResponse> CreateServiceAsync(CreateServiceDiscoveryServiceRequest createServiceDiscoveryRequest, CancellationToken ct = default)
        {
            CreateServiceResponse resp = default;
            try
            {
                resp = await _awsServiceDiscoveryClient.CreateServiceAsync(new CreateServiceRequest
                {
                    Name = createServiceDiscoveryRequest.Name,
                    NamespaceId = createServiceDiscoveryRequest.NamespaceId,
                    DnsConfig = new()
                    {
                        DnsRecords = createServiceDiscoveryRequest.DnsConfig.CloudMapDnsRecords.Select(record => new DnsRecord
                        {
                            TTL = record.TTL,
                            Type = record.Type
                        }).ToList()
                    }
                });

                ServiceDiscoveryId = resp.Service != null ? resp.Service.Id : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create service discovery service.");
            }

            return resp;
        }

        public async Task<DeleteServiceResponse> DeleteServiceAsync(DeleteServiceDiscoveryServiceRequest deleteServiceDiscoveryRequest, CancellationToken ct = default)
        {
            DeleteServiceResponse resp = default;
            try
            {
                resp = await _awsServiceDiscoveryClient.DeleteServiceAsync(new DeleteServiceRequest
                {
                    Id = deleteServiceDiscoveryRequest.Id
                });
            }
            catch (Amazon.ServiceDiscovery.Model.ResourceInUseException ex)
            {
                _logger.LogWarning(ex, "Failed to delete service discovery service. Aws resource is in use. Rethrowing this exception");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete service discovery service. Returning an empty response.");
            }

            return resp;
        }

        public async Task<GetNamespaceResponse> GetNamespaceAsync(GetCloudMapNamespaceRequest getNamespaceRequest, CancellationToken ct = default)
        {
            GetNamespaceResponse resp = default;
            try
            {
                resp = await _awsServiceDiscoveryClient.GetNamespaceAsync(new GetNamespaceRequest
                {
                    Id = getNamespaceRequest.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get aws cloud map namespace details.");
            }

            return resp;
        }

        public async Task<string> RollbackCloudMapDiscoveryServiceAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(ServiceDiscoveryId) == false)
            {
                DeleteServiceDiscoveryServiceRequest request = new DeleteServiceDiscoveryServiceRequest
                {
                    Id = ServiceDiscoveryId
                };
                Amazon.ServiceDiscovery.Model.DeleteServiceResponse response;
                try
                {
                    response = await DeleteServiceAsync(request);
                }
                catch (Amazon.ServiceDiscovery.Model.ResourceInUseException ex)
                {
                    response = await RollbackCloudMapDiscoveryServiceRetryAsync(request, ct);
                }

                ServiceDiscoveryId = response == null ? ServiceDiscoveryId : string.Empty;
            }

            return ServiceDiscoveryId;
        }

        private async Task<Amazon.ServiceDiscovery.Model.DeleteServiceResponse> RollbackCloudMapDiscoveryServiceRetryAsync(DeleteServiceDiscoveryServiceRequest request, CancellationToken ct)
        {
            Amazon.ServiceDiscovery.Model.DeleteServiceResponse response = default;
            Stopwatch mainStopwatch = new Stopwatch();
            Stopwatch intervalStopWatch = new Stopwatch();
            mainStopwatch.Start();
            intervalStopWatch.Start();
            while (mainStopwatch.Elapsed.TotalSeconds <= DefaultTimeToWait_InSeconds)
            {
                if (intervalStopWatch.Elapsed.TotalSeconds > 15)
                {
                    intervalStopWatch.Stop();
                    try
                    {
                        response = await DeleteServiceAsync(request, ct);

                        mainStopwatch.Stop();
                        intervalStopWatch.Stop();
                        _logger.LogInformation("Successfully deleted Service Discovery Service after retries.");
                        break;
                    }
                    catch (Amazon.ServiceDiscovery.Model.ResourceInUseException ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        intervalStopWatch.Restart();
                    }
                }
            }
            return response;
        }
    }
}
