using Amazon.ECS.Model;
using Leadsly.Domain.Services;
using Leadsly.Models;
using Leadsly.Models.Aws;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CloudPlatformProvider : ICloudPlatformProvider
    {
        public CloudPlatformProvider(IAwsElasticContainerService awsElasticContainerService, ILogger<CloudPlatformProvider> logger)
        {
            _awsElasticContainerService = awsElasticContainerService;
            _logger = logger;
        }

        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly ILogger<CloudPlatformProvider> _logger;

        public async Task<CloudPlatformOperationResult> SetupNewUsersContainerAsync(SetupNewUserInLeadslyDTO createContainer, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new CloudPlatformOperationResult();

            // 1. RunTask for this user to create the container                    
            RunTaskResponse runTaskResponse = await RunTaskAsync(createContainer.EcsTask, ct);

            if(runTaskResponse == null)
            {
                result.Succeeded = false;
                return result;
            }

            if(runTaskResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result = HandleUserSetupErrors(runTaskResponse.Failures);
                return result;
            }

            // 2. Check the current desired count for the service            
            DescribeServicesResponse describeTaskResponse = await DescribeServiceAsync(createContainer.EcsService, ct);

            if(describeTaskResponse == null)
            {
                result.Succeeded = false;
                return result;
            }
            
            if(describeTaskResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result = HandleUserSetupErrors(describeTaskResponse.Failures);
                return result;
            }

            // 3. Update the desired count by one for this service
            if(describeTaskResponse.Services.Count > 1)
            {
                _logger.LogWarning("Expected to find one service, but found more.");
            }

            Service mainService = describeTaskResponse.Services.FirstOrDefault();
            if(mainService == null)
            {
                result.Succeeded = false;
                result.Failures = new()
                {
                    new() { Reason = "No services found", Detail = "The provided service name was not found in aws" }
                };
                return result;
            }

            ECSServiceDTO service = new()
            {
                ClusterArn = mainService.ClusterArn,
                ServiceName = mainService.ServiceName, 
                DesiredCount = mainService.DesiredCount
            };

            UpdateServiceResponse updateTaskResponse = await UpdateServiceAsync(service, ct);
            if(updateTaskResponse == null)
            {
                result.Succeeded = false;                
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private CloudPlatformOperationResult HandleUserSetupErrors(List<Failure> failures)
        {
            CloudPlatformOperationResult result = new CloudPlatformOperationResult();
            _logger.LogError("Failed to setup users container.");
            result.Failures = new();
            foreach (Failure failure in failures)
            {
                result.Failures.Add(new()
                {
                    Detail = failure.Detail,
                    Reason = failure.Reason,
                    Arn = failure.Arn
                });
                _logger.LogError("Reason: ", failure.Reason);
            }

            result.Succeeded = false;

            return result;
        }

        private async Task<RunTaskResponse> RunTaskAsync(ECSTaskDTO ecsTask, CancellationToken ct = default)
        {
            RunEcsTaskRequest request = new()
            {
                AssignPublicIp = Enum.GetName(ecsTask.AssignPublicIp).ToUpper(),
                Cluster = ecsTask.Cluster,
                Count = ecsTask.Count,
                LaunchType = Enum.GetName(ecsTask.LaunchType).ToUpper(),
                Subnets = ecsTask.Subnets,
                TaskDefinition = ecsTask.TaskDefinition                
            };

            return await _awsElasticContainerService.RunTaskAsync(request, ct);

        }

        private async Task<DescribeServicesResponse> DescribeServiceAsync(ECSServiceDTO service, CancellationToken ct = default)
        {
            List<string> services = new() { service.Service };
            string cluster = service.Cluster;

            DescribeEcsServiceRequest request = new()
            {
                Cluster = cluster,
                Services = services
            };

            return await _awsElasticContainerService.DescribeServiceAsync(request, ct);
        }

        private async Task<UpdateServiceResponse> UpdateServiceAsync(ECSServiceDTO service, CancellationToken ct = default)
        {
            UpdateEcsServiceRequest request = new()
            {
                ClusterArn = service.ClusterArn,
                DesiredCount = service.DesiredCount,
                ServiceName = service.ServiceName
            };

            return await _awsElasticContainerService.UpdateServiceAsync(request, ct);
        }


        public async Task<CloudPlatformOperationResult> SetupExistingContainerAsync(SetupExistingUserInLeadslyDTO createContainer, CancellationToken ct = default)
        {

            var containerInfoDTO = new CloudPlatformOperationResult();
            return containerInfoDTO;
        }
    }
}
