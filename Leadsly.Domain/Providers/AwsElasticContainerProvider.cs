using Leadsly.Domain.Services;
using Leadsly.Models;
using Leadsly.Models.Aws;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class AwsElasticContainerProvider : IAwsElasticContainerProvider
    {
        public AwsElasticContainerProvider(IAwsElasticContainerService awsElasticContainerService, ILogger<AwsElasticContainerProvider> logger)
        {
            _awsElasticContainerService = awsElasticContainerService;
            _logger = logger;
        }

        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly ILogger<AwsElasticContainerProvider> _logger;

        public async Task<AwsOperationResult> SetupNewUsersContainerAsync(SetupNewUserInLeadslyDTO createContainer, CancellationToken ct = default)
        {
            CreateEcsServiceRequest createServiceRequest = new CreateEcsServiceRequest
            {
                DesiredCount = createContainer.EcsService.DesiredCount,
                ServiceName = createContainer.EcsService.Service,
                TaskDefinition = createContainer.EcsService.TaskDefinition,
                Cluster = createContainer.EcsService.Cluster,
                AssignPublicIp = Enum.GetName(createContainer.EcsService.AssignPublicIp).ToUpper(),
                LaunchType = Enum.GetName(createContainer.EcsService.LaunchType).ToUpper(),                
                SchedulingStrategy = createContainer.EcsService.SchedulingStrategy
            };

            var createEcsServiceResponse = await _awsElasticContainerService.CreateServiceAsync(createServiceRequest, ct);

            if(createEcsServiceResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                // issue occured
            }
            else
            {
                // create the task next

                RunEcsTaskRequest runTaskRequest = new RunEcsTaskRequest
                {
                    LaunchType = Enum.GetName(createContainer.EcsTask.LaunchType).ToUpper(),
                    AssignPublicIp = Enum.GetName(createContainer.EcsTask.AssignPublicIp).ToUpper(),
                    TaskDefinition = createContainer.EcsTask.TaskDefinition,
                    Cluster = createContainer.EcsTask.Cluster,
                    Count = createContainer.EcsTask.Count,
                };

                var createEcsTaskResponse = await _awsElasticContainerService.RunTaskAsync(runTaskRequest, ct);
            }   

            return new AwsOperationResult
            {

            };
        }


        public async Task<AwsOperationResult> SetupExistingContainerAsync(SetupExistingUserInLeadslyDTO createContainer, CancellationToken ct = default)
        {

            var containerInfoDTO = new AwsOperationResult();
            return containerInfoDTO;
        }
    }
}
