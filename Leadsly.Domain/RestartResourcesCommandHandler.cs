using Amazon.ECS.Model;
using Leadsly.Application.Model.Aws.ElasticContainerService;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Leadsly.Domain
{
    public class RestartResourcesCommandHandler : ICommandHandler<RestartResourcesCommand>
    {
        public RestartResourcesCommandHandler(IAwsElasticContainerService awsEcsService, IVirtualAssistantRepository virtualAssistantRepository, ILogger<RestartResourcesCommandHandler> logger, ICloudPlatformRepository cloudPlatformRepository)
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _logger = logger;
            _awsEcsService = awsEcsService;
            _cloudPlatformRepository = cloudPlatformRepository;
        }

        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly IAwsElasticContainerService _awsEcsService;
        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly ILogger<RestartResourcesCommandHandler> _logger;

        public async System.Threading.Tasks.Task HandleAsync(RestartResourcesCommand command)
        {
            string halId = command.HalId;

            _logger.LogDebug("Executing RestartResourceCommand for Hal Id {halId}", halId);

            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId);

            if (virtualAssistant != null)
            {
                _logger.LogInformation("Successfully found Virtual Assistant with Hal Id {halId}", halId);
                if (virtualAssistant.EcsServices != null && virtualAssistant.EcsServices.Count > 0)
                {
                    EcsService halEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == Purpose.Hal);

                    // since the task name will be stale after the first restart (new task arn will be assigned)
                    // we need to get fresh list of tasks for this ecs service name to be able to restart them
                    ListTasksRequest listTasksRequest = new()
                    {
                        Cluster = halEcsService.ClusterArn,
                        ServiceName = halEcsService.ServiceName
                    };
                    ListTasksResponse listTasksResponse = await _awsEcsService.ListTasksAsync(listTasksRequest);

                    if (listTasksResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string serviceName = halEcsService.ServiceName;
                        string numberOfTasks = listTasksResponse.TaskArns.Count().ToString();
                        _logger.LogDebug("ListTasks request to AWS succeeded. Service name used was {serviceName} and number of tasks returned was {numberOfTasks}", serviceName, numberOfTasks);
                        foreach (string taskArn in listTasksResponse.TaskArns)
                        {
                            StopEcsTaskRequest request = new()
                            {
                                Cluster = halEcsService.ClusterArn,
                                Reason = "Routine restart",
                                Task = taskArn
                            };

                            string ecsServiceName = halEcsService.ServiceName;
                            _logger.LogDebug("Sending request to stop all tasks running under Hal's ECS service {ecsServiceName}", ecsServiceName);

                            StopTaskResponse stopTaskResponse = await _awsEcsService.StopTaskAsync(request);
                            if (stopTaskResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                            {
                                _logger.LogDebug("Stopped task {taskArn}", taskArn);
                            }
                            else
                            {
                                _logger.LogError("Error stopping task {taskArn}", taskArn);
                            }

                            // update ecs service tasks arn
                            _logger.LogInformation("Updating Ecs Tasks with latest task arn");
                            IList<EcsTask> updatedEcsTasks = halEcsService.EcsTasks.Select(x =>
                            {
                                return new EcsTask
                                {
                                    EcsService = halEcsService,
                                    EcsServiceId = halEcsService.EcsServiceId,
                                    TaskArn = taskArn,
                                    EcsTaskId = x.EcsTaskId
                                };
                            }).ToList();

                            await _cloudPlatformRepository.UpdateEcsTasksAsync(updatedEcsTasks);
                        }
                    }
                    else
                    {
                        string serviceName = halEcsService.ServiceName;
                        _logger.LogWarning("Error listing tasks for service {serviceName}. HalId {halId}", serviceName, halId);
                    }
                }
                else
                {
                    _logger.LogWarning("No ECS services found for Virtual Assistant with Hal Id {halId}", halId);
                }
            }
            else
            {
                _logger.LogInformation("Failed to find VirtualAssistant for HalId {halId}", halId);
            }
        }
    }
}
