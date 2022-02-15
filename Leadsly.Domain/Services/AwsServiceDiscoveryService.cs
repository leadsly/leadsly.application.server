using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
using Leadsly.Models.Aws;
using Leadsly.Models.Aws.ServiceDiscovery;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete service discovery service.");
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
    }
}
