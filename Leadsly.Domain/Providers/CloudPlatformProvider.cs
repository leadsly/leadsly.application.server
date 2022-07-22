using Amazon.ECS.Model;
using Amazon.ServiceDiscovery.Model;
using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Leadsly.Domain.Providers
{
    public class CloudPlatformProvider : ICloudPlatformProvider
    {
        public CloudPlatformProvider(
            IAwsElasticContainerService awsElasticContainerService,
            ICloudPlatformRepository cloudPlatformRepository,
            IAwsServiceDiscoveryService awsServiceDiscoveryService,
            IVirtualAssistantRepository virtualAssistantRepository,
            IOrphanedCloudResourcesRepository orphanedCloudResourcesRepository,
            ILogger<CloudPlatformProvider> logger)
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _awsElasticContainerService = awsElasticContainerService;
            _awsServiceDiscoveryService = awsServiceDiscoveryService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _orphanedCloudResourcesRepository = orphanedCloudResourcesRepository;
            _logger = logger;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly IAwsServiceDiscoveryService _awsServiceDiscoveryService;
        private readonly IOrphanedCloudResourcesRepository _orphanedCloudResourcesRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILogger<CloudPlatformProvider> _logger;
        private readonly int DefaultTimeToWaitForEcsServicePendingTasks_InSeconds = 160;

        public async Task DeleteAwsEcsServiceAsync(string userId, string serviceName, string clusterName, CancellationToken ct = default)
        {
            bool succeeded = await _awsElasticContainerService.DeleteServiceAsync(serviceName, clusterName, ct);
            if (succeeded == false)
            {
                _logger.LogDebug("Delete operation for ecs service failed. Adding ecs service to orphaned cloud resources table for manual clean up.");
                await SaveOrphanedResourcesAsync(userId, serviceName, "Ecs Service", ct);
            }
        }

        public async Task DeleteEcsServiceAsync(string ecsServiceId, CancellationToken ct = default)
        {
            await _cloudPlatformRepository.RemoveEcsServiceAsync(ecsServiceId, ct);
        }

        public async Task DeleteAwsTaskDefinitionRegistrationAsync(string userId, string taskDefinitionFamily, CancellationToken ct = default)
        {
            bool succeeded = await _awsElasticContainerService.DeleteTaskDefinitionRegistrationAsync(taskDefinitionFamily, ct);
            if (succeeded == false)
            {
                _logger.LogDebug("Operation to remove aws cloud map discovery service failed. Adding cloud map discovery service to orphaned cloud resources table for manual clean up.");
                await SaveOrphanedResourcesAsync(userId, taskDefinitionFamily, "Task Definition", ct);
            }
        }

        public async Task DeleteTaskDefinitionRegistrationAsync(string taskDefinitionId, CancellationToken ct = default)
        {
            await _cloudPlatformRepository.RemoveEcsTaskDefinitionAsync(taskDefinitionId, ct);
        }

        public async Task DeleteAwsCloudMapServiceAsync(string userId, string serviceDiscoveryId, CancellationToken ct = default)
        {
            bool succeeded = await _awsServiceDiscoveryService.DeleteCloudMapDiscoveryServiceAsync(serviceDiscoveryId, ct);
            if (succeeded == false)
            {
                _logger.LogDebug("Deregister operation for ecs task definition failed. Adding task definition to orphaned cloud resources table for manual clean up.");
                await SaveOrphanedResourcesAsync(userId, serviceDiscoveryId, "CloudMapDiscoveryService", ct);
            }
        }

        public async Task DeleteCloudMapServiceAsync(string cloudMapServiceId, CancellationToken ct = default)
        {
            await _cloudPlatformRepository.RemoveCloudMapServiceDiscoveryServiceAsync(cloudMapServiceId, ct);
        }

        private async Task SaveOrphanedResourcesAsync(string userId, string resource, string friendslyName, CancellationToken ct = default)
        {
            OrphanedCloudResource orphanedCloudResource = new()
            {
                UserId = userId,
                FriendlyName = friendslyName,
                ResourceId = resource
            };

            await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
        }

        public async Task<EcsService> CreateEcsServiceInAwsAsync(string taskDefinition, string cloudMapServiceArn, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            Amazon.ECS.Model.CreateServiceRequest request = new Amazon.ECS.Model.CreateServiceRequest
            {
                DesiredCount = configuration.EcsServiceConfig.DesiredCount,
                ServiceName = $"hal-{Guid.NewGuid()}-service",
                TaskDefinition = taskDefinition,
                Cluster = configuration.EcsServiceConfig.ClusterArn,
                LaunchType = configuration.EcsServiceConfig.LaunchType,
                ServiceRegistries = new List<ServiceRegistry>()
                {
                    new()
                    {
                        RegistryArn = cloudMapServiceArn
                    }
                },
                NetworkConfiguration = new()
                {
                    AwsvpcConfiguration = new()
                    {
                        AssignPublicIp = configuration.EcsServiceConfig.AssignPublicIp,
                        Subnets = configuration.EcsServiceConfig.Subnets,
                        SecurityGroups = configuration.EcsServiceConfig.SecurityGroups
                    }
                },
                SchedulingStrategy = configuration.EcsServiceConfig.SchedulingStrategy
            };

            Amazon.ECS.Model.CreateServiceResponse response = await _awsElasticContainerService.CreateServiceAsync(request, ct);
            if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create ECS Service in AWS");
                return null;
            }

            _logger.LogInformation("Successfully created ECS Service in AWS");

            // copy properties from response.Service to new EcsCreateService object
            EcsService ecsCreateService = new()
            {
                ClusterArn = response.Service.ClusterArn,
                CreatedAt = ((DateTimeOffset)response.Service.CreatedAt).ToUnixTimeSeconds(),
                CreatedBy = response.Service.CreatedBy,
                EcsServiceRegistries = response.Service.ServiceRegistries.Select(r => new EcsServiceRegistry()
                {
                    RegistryArn = r.RegistryArn,
                }).ToList(),
                SchedulingStrategy = response.Service.SchedulingStrategy,
                ServiceArn = response.Service.ServiceArn,
                ServiceName = response.Service.ServiceName,
                TaskDefinition = response.Service.TaskDefinition,
            };

            return ecsCreateService;
        }

        public async Task<CloudMapDiscoveryService> CreateCloudMapDiscoveryServiceInAwsAsync(CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            string serviceDiscoveryName = $"hal-{Guid.NewGuid()}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceRequest request = new Amazon.ServiceDiscovery.Model.CreateServiceRequest
            {
                Name = serviceDiscoveryName,
                NamespaceId = configuration.ServiceDiscoveryConfig.NamespaceId,
                DnsConfig = new()
                {
                    DnsRecords = new List<DnsRecord>()
                    {
                        new DnsRecord()
                        {
                            TTL = configuration.ServiceDiscoveryConfig.DnsRecordTTL,
                            Type = configuration.ServiceDiscoveryConfig.DnsRecordType
                        }
                    }
                }
            };

            Amazon.ServiceDiscovery.Model.CreateServiceResponse response = await _awsServiceDiscoveryService.CreateServiceAsync(request, ct);
            if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create Cloud Map service discovery service in AWS. HttpStatusCode: {response?.HttpStatusCode}");
                return null;
            }
            _logger.LogInformation($"Successfully created Cloud Map service discovery service in AWS. HttpStatusCode: {response?.HttpStatusCode}");

            CloudMapDiscoveryService cloudMapDiscoveryService = new()
            {
                Arn = response.Service.Arn,
                CreateDate = response.Service.CreateDate,
                Name = serviceDiscoveryName,
                ServiceDiscoveryId = response.Service.Id
            };

            return cloudMapDiscoveryService;
        }

        public async Task<EcsTaskDefinition> RegisterTaskDefinitionInAwsAsync(string halId, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            string taskDefinition = $"{halId}-task-def";
            string gridContainerName = $"{halId}-gird";

            RegisterTaskDefinitionRequest request = new RegisterTaskDefinitionRequest
            {
                ContainerDefinitions = configuration.EcsTaskDefinitionConfig.ContainerDefinitions.Select(cd => new Amazon.ECS.Model.ContainerDefinition
                {
                    Cpu = cd.Cpu,
                    DisableNetworking = cd.DisableNetworking,
                    DependsOn = cd.DependsOn?.Select(x => new Amazon.ECS.Model.ContainerDependency
                    {
                        Condition = x.Condition,
                        ContainerName = x.ContainerName
                    }).ToList(),
                    Essential = cd.Essential,
                    Image = cd.Image,
                    Memory = cd.Memory,
                    Name = cd.Name,
                    PortMappings = cd.PortMappings?.Select(x => new Amazon.ECS.Model.PortMapping
                    {
                        ContainerPort = x.ContainerPort,
                        HostPort = x.HostPort
                    }).ToList(),
                    Privileged = cd.Privileged,
                    RepositoryCredentials = new Amazon.ECS.Model.RepositoryCredentials
                    {
                        CredentialsParameter = cd.RepositoryCredentials.CredentialsParameter
                    },
                    StartTimeout = cd.StartTimeout,
                    StopTimeout = cd.StopTimeout,
                    VolumesFrom = cd.VolumesFrom.Select(x => new Amazon.ECS.Model.VolumeFrom
                    {
                        ReadOnly = x.ReadOnly,
                        SourceContainer = x.SourceContainer
                    }).ToList()
                }).ToList(),
                Cpu = configuration.EcsTaskDefinitionConfig.Cpu,
                ExecutionRoleArn = configuration.EcsTaskDefinitionConfig.ExecutionRoleArn,
                Family = taskDefinition,
                Memory = configuration.EcsTaskDefinitionConfig.Memory,
                NetworkMode = configuration.EcsTaskDefinitionConfig.NetworkMode,
                RequiresCompatibilities = configuration.EcsTaskDefinitionConfig.RequiresCompatibilities.ToList(),
                TaskRoleArn = configuration.EcsTaskDefinitionConfig.TaskRoleArn

            };

            RegisterTaskDefinitionResponse response = await _awsElasticContainerService.RegisterTaskDefinitionAsync(request, ct);
            if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to register ECS task definition in AWS");
                return null;
            }
            _logger.LogInformation("Successfully registered ECS task definition in AWS.");

            EcsTaskDefinition ecsTaskDefinition = new()
            {
                TaskDefinitionArn = response.TaskDefinition.TaskDefinitionArn,
                Family = response.TaskDefinition.Family
            };

            return ecsTaskDefinition;
        }

        private async Task<Amazon.ECS.Model.DescribeServicesResponse> DescribeServicesAsync(string serviceName, string clusterArn, CancellationToken ct = default)
        {
            DescribeServicesRequest request = new DescribeServicesRequest
            {
                Cluster = clusterArn,
                Services = new List<string> { serviceName }
            };

            return await _awsElasticContainerService.DescribeServicesAsync(request, ct);
        }

        public async Task<bool> EnsureEcsServiceTasksAreRunningAsync(string ecsServiceName, string clusterArn, CancellationToken ct = default)
        {
            Stopwatch mainStopWatch = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            mainStopWatch.Start();
            _logger.LogInformation("Ensuring ecs service tasks are running. Waiting 20 seconds before checking.");
            while (mainStopWatch.Elapsed.TotalSeconds <= DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
            {
                // Check elapsed time w/o stopping/resetting the stopwatch                
                if (stopwatch.Elapsed.TotalSeconds >= 20)
                {
                    double timeout = (DefaultTimeToWaitForEcsServicePendingTasks_InSeconds - mainStopWatch.Elapsed.TotalSeconds);
                    _logger.LogInformation("Checking if ecs service tasks are running... Times out in: {timeout}", timeout);
                    // At least 5 seconds elapsed, restart stopwatch.
                    stopwatch.Stop();
                    CloudPlatformOperationResult checkTaskStatusResult = await AreEcsServiceTasksRunningAsync(ecsServiceName, clusterArn, ct);
                    if (checkTaskStatusResult.Succeeded)
                    {
                        if (((bool)checkTaskStatusResult.Value) == true)
                        {
                            _logger.LogInformation("Successfully verified ecs service tasks are running");
                            mainStopWatch.Stop();
                            stopwatch.Stop();
                            return true;
                        }
                        else
                        {
                            _logger.LogInformation("Ecs service tasks are not running yet. Checking again in 20 seconds...");
                        }
                    }
                    else
                    {
                        return false;
                    }
                    stopwatch.Restart();
                }

                if (mainStopWatch.Elapsed.TotalSeconds == DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
                {
                    _logger.LogWarning("Failed to find out if ecs service tasks are running in the alotted time.");
                    return false;
                }
            }

            return true;
        }

        private async Task<CloudPlatformOperationResult> AreEcsServiceTasksRunningAsync(string ecsServiceName, string clusternArn, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            Amazon.ECS.Model.DescribeServicesResponse response = await DescribeServicesAsync(ecsServiceName, clusternArn, ct);

            if (response == null || response.Services == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Value = false;
                result.Failures.Add(new()
                {
                    Arn = ecsServiceName,
                    Code = Leadsly.Application.Model.Codes.AWS_API_ERROR,
                    Detail = "Aws API returned null, services were null or status code did not equal 200",
                    Reason = "Failed to get number of pending tasks for this service"
                });
                return result;
            }

            if (response.Services.Count() > 1)
            {
                _logger.LogWarning("Expected to find a single service in the response but more than one were found. Using first service in the collection to check if all tasks are running");
            }

            result.Succeeded = true;
            result.Value = response.Services.First().PendingCount == 0 && response.Services.First().RunningCount > 0;
            return result;
        }

        public async Task<VirtualAssistant> GetVirtualAssistantAsync(string userId, CancellationToken ct = default)
        {
            IList<VirtualAssistant> virtualAssistants = await _virtualAssistantRepository.GetAllByUserIdAsync(userId, ct);

            // for now we only expect users to have only one assistant.
            return virtualAssistants.FirstOrDefault();
        }

        public async Task<VirtualAssistant> CreateVirtualAssistantAsync(
            EcsTaskDefinition newEcsTaskDefinition,
            EcsService newEcsService,
            IList<EcsTask> ecsServiceTasks,
            CloudMapDiscoveryService newCloudMapService,
            string halId,
            string userId,
            string timezoneId,
            CancellationToken ct = default)
        {
            HalUnit newHalUnit = new()
            {
                HalId = halId,
                TimeZoneId = timezoneId,
                ApplicationUserId = userId
            };

            newCloudMapService.EcsService = newEcsService;
            newEcsService.CloudMapDiscoveryService = newCloudMapService;
            newEcsService.EcsTasks = ecsServiceTasks;

            VirtualAssistant virtualAssistant = new VirtualAssistant
            {
                EcsTaskDefinition = newEcsTaskDefinition,
                EcsService = newEcsService,
                CloudMapDiscoveryService = newCloudMapService,
                ApplicationUserId = userId,
                HalUnit = newHalUnit,
                HalId = halId,
            };

            return await _virtualAssistantRepository.CreateAsync(virtualAssistant, ct);
        }

        public async Task<bool> DeleteVirtualAssistantAsync(string virtualAssistantId, CancellationToken ct = default)
        {
            return await _virtualAssistantRepository.DeleteAsync(virtualAssistantId, ct);
        }

        public async Task<IList<EcsTask>> ListEcsServiceTasksAsync(string clusterArn, string serviceArn, CancellationToken ct = default)
        {
            ListTasksRequest request = new ListTasksRequest
            {
                Cluster = clusterArn,
                ServiceName = serviceArn
            };

            ListTasksResponse response = await _awsElasticContainerService.ListTasksAsync(request, ct);
            if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to list ECS tasks in AWS");
                return null;
            }

            _logger.LogInformation("Successfully listed ECS tasks in AWS.");
            IList<EcsTask> tasks = response.TaskArns.Select(taskArn => new EcsTask
            {
                TaskArn = taskArn
            }).ToList();

            return tasks;
        }

    }
}
