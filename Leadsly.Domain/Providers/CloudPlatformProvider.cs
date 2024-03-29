﻿using Amazon.ECS.Model;
using Amazon.ServiceDiscovery.Model;
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
            IAwsS3Service awsS3Service,
            IVirtualAssistantRepository virtualAssistantRepository,
            IOrphanedCloudResourcesRepository orphanedCloudResourcesRepository,
            ILogger<CloudPlatformProvider> logger)
        {
            _awsS3Service = awsS3Service;
            _virtualAssistantRepository = virtualAssistantRepository;
            _awsElasticContainerService = awsElasticContainerService;
            _awsServiceDiscoveryService = awsServiceDiscoveryService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _orphanedCloudResourcesRepository = orphanedCloudResourcesRepository;
            _logger = logger;
        }

        private readonly IAwsS3Service _awsS3Service;
        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly IAwsServiceDiscoveryService _awsServiceDiscoveryService;
        private readonly IOrphanedCloudResourcesRepository _orphanedCloudResourcesRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILogger<CloudPlatformProvider> _logger;
        private readonly int DefaultTimeToWaitForEcsServicePendingTasks_InSeconds = 240;

        public async Task DeleteAwsEcsServiceAsync(string userId, string serviceName, string clusterName, CancellationToken ct = default)
        {
            bool succeeded = await _awsElasticContainerService.DeleteServiceAsync(serviceName, clusterName, ct);
            if (succeeded == false)
            {
                _logger.LogDebug("Delete operation for ecs service failed. Adding ecs service to orphaned cloud resources table for manual clean up.");
                await SaveOrphanedResourcesAsync(userId, serviceName, "Ecs Service", ct);
            }
        }

        public async Task<bool> DeleteEcsTasksByEcsServiceId(string ecsServiceId, CancellationToken ct = default)
        {
            return await _cloudPlatformRepository.RemoveEcsTasksByServiceIdAsync(ecsServiceId, ct);
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

        public async Task DeleteAwsS3HalDirectoryAsync(string halId, CancellationToken ct = default)
        {
            if (await _awsS3Service.DeleteDirectoryAsync(halId, ct) == false)
            {
                _logger.LogDebug("Operation to delete {halId} S3 directory failed", halId);
            }
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

        public async Task<Amazon.ECS.Model.CreateServiceResponse> CreateEcsServiceInAwsAsync(string serviceName, string taskDefinition, string cloudMapServiceArn, Config ecsServiceConfig, CancellationToken ct = default)
        {
            Amazon.ECS.Model.CreateServiceRequest request = new Amazon.ECS.Model.CreateServiceRequest
            {
                DesiredCount = ecsServiceConfig.DesiredCount,
                ServiceName = serviceName,
                TaskDefinition = taskDefinition,
                Cluster = ecsServiceConfig.ClusterArn,
                LaunchType = ecsServiceConfig.LaunchType,
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
                        AssignPublicIp = ecsServiceConfig.AssignPublicIp,
                        Subnets = ecsServiceConfig.Subnets,
                        SecurityGroups = ecsServiceConfig.SecurityGroups
                    }
                },
                SchedulingStrategy = ecsServiceConfig.SchedulingStrategy
            };

            return await _awsElasticContainerService.CreateServiceAsync(request, ct);
        }

        public async Task<Amazon.ServiceDiscovery.Model.CreateServiceResponse> CreateCloudMapDiscoveryServiceInAwsAsync(string serviceDiscoveryName, CloudMapConfig cloudMapConfig, CancellationToken ct = default)
        {
            Amazon.ServiceDiscovery.Model.CreateServiceRequest request = new Amazon.ServiceDiscovery.Model.CreateServiceRequest
            {
                Name = serviceDiscoveryName,
                NamespaceId = cloudMapConfig.NamespaceId,
                DnsConfig = new()
                {
                    DnsRecords = new List<DnsRecord>()
                    {
                        new DnsRecord()
                        {
                            TTL = cloudMapConfig.DnsRecordTTL,
                            Type = cloudMapConfig.DnsRecordType
                        }
                    }
                }
            };

            return await _awsServiceDiscoveryService.CreateServiceAsync(request, ct);
        }

        public async Task<Amazon.ServiceDiscovery.Model.RegisterInstanceResponse> RegisterCloudMapSrvInstanceAsync(string ecsServiceId, CancellationToken ct = default)
        {
            Amazon.ServiceDiscovery.Model.RegisterInstanceRequest request = new RegisterInstanceRequest
            {
                Attributes = new Dictionary<string, string>
                {
                    { "AWS_INSTANCE_PORT", "80" }
                },
                CreatorRequestId = Guid.NewGuid().ToString(),
                ServiceId = ecsServiceId,
                InstanceId = ecsServiceId
            };

            return await _awsServiceDiscoveryService.RegisterInstanceAsync(request, ct);
        }

        public async Task<RegisterTaskDefinitionResponse> RegisterHalTaskDefinitionInAwsAsync(string halTaskDefinition, string halId, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            RegisterTaskDefinitionRequest request = new RegisterTaskDefinitionRequest
            {
                ContainerDefinitions = configuration.EcsHalTaskDefinitionConfig.ContainerDefinitions?.Select(cd =>
                {
                    string awsLogsGroup = cd.LogConfiguration.Options.AwslogsGroup.Replace("{halId}", halId);

                    List<Amazon.ECS.Model.KeyValuePair> environmentVars = new List<Amazon.ECS.Model.KeyValuePair>();
                    if (cd.Environment != null)
                    {
                        environmentVars = cd.Environment.Select(y => new Amazon.ECS.Model.KeyValuePair
                        {
                            Name = y.Name,
                            Value = y.Value
                        }).ToList();
                    }

                    environmentVars.Add(new Amazon.ECS.Model.KeyValuePair
                    {
                        Name = "HAL_ID",
                        Value = halId
                    });

                    Amazon.ECS.Model.ContainerDefinition containerDef = new Amazon.ECS.Model.ContainerDefinition
                    {
                        Cpu = cd.Cpu,
                        DisableNetworking = cd.DisableNetworking,
                        Environment = environmentVars,
                        DependsOn = cd.DependsOn?.Select(x => new Amazon.ECS.Model.ContainerDependency
                        {
                            Condition = x.Condition,
                            ContainerName = x.ContainerName
                        }).ToList(),
                        Image = cd.Image,
                        Memory = cd.Memory,
                        Name = cd.Name,
                        HealthCheck = new()
                        {
                            Command = cd.HealthCheck.Command.ToList(),
                            Interval = cd.HealthCheck.Interval,
                            Retries = cd.HealthCheck.Retries,
                            Timeout = cd.HealthCheck.Timeout,
                            StartPeriod = cd.HealthCheck.StartPeriod
                        },
                        LogConfiguration = new Amazon.ECS.Model.LogConfiguration
                        {
                            LogDriver = cd.LogConfiguration?.LogDriver,
                            Options = new Dictionary<string, string>
                            {
                                { "awslogs-group", awsLogsGroup },
                                { "awslogs-stream-prefix", cd.LogConfiguration.Options.AwslogsStreamPrefix },
                                { "awslogs-region", cd.LogConfiguration.Options.AwslogsRegion },
                                { "awslogs-create-group", cd.LogConfiguration.Options.AwslogsCreateGroup }
                            }
                        },
                        LinuxParameters = new Amazon.ECS.Model.LinuxParameters
                        {
                            InitProcessEnabled = cd.LinuxParameters.InitProcessEnabled
                        },
                        PortMappings = cd.PortMappings?.Select(x => new Amazon.ECS.Model.PortMapping
                        {
                            ContainerPort = x.ContainerPort,
                            HostPort = x.HostPort
                        }).ToList(),
                        Privileged = cd.Privileged
                    };

                    if (cd.LinuxParameters.SharedMemorySize > 0)
                    {
                        containerDef.LinuxParameters.SharedMemorySize = cd.LinuxParameters.SharedMemorySize;
                    }

                    return containerDef;
                }).ToList(),
                Cpu = configuration.EcsHalTaskDefinitionConfig.Cpu,
                ExecutionRoleArn = configuration.EcsHalTaskDefinitionConfig.ExecutionRoleArn,
                Family = halTaskDefinition,
                Memory = configuration.EcsHalTaskDefinitionConfig.Memory,
                NetworkMode = configuration.EcsHalTaskDefinitionConfig.NetworkMode,
                RequiresCompatibilities = configuration.EcsHalTaskDefinitionConfig.RequiresCompatibilities.ToList(),
                TaskRoleArn = configuration.EcsHalTaskDefinitionConfig.TaskRoleArn
            };

            return await _awsElasticContainerService.RegisterTaskDefinitionAsync(request, ct);
        }

        public async Task<RegisterTaskDefinitionResponse> RegisterGridTaskDefinitionInAwsAsync(string gridTaskDefinition, string halId, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            RegisterTaskDefinitionRequest request = new RegisterTaskDefinitionRequest
            {
                ContainerDefinitions = configuration.EcsGridTaskDefinitionConfig.ContainerDefinitions?.Select(cd =>
                {
                    string awsLogsGroup = cd.LogConfiguration.Options.AwslogsGroup.Replace("{halId}", halId);

                    List<Amazon.ECS.Model.KeyValuePair> environmentVars = new List<Amazon.ECS.Model.KeyValuePair>();
                    if (cd.Environment != null)
                    {
                        environmentVars = cd.Environment.Select(y => new Amazon.ECS.Model.KeyValuePair
                        {
                            Name = y.Name,
                            Value = y.Value
                        }).ToList();
                    }

                    environmentVars.Add(new Amazon.ECS.Model.KeyValuePair
                    {
                        Name = "HAL_ID",
                        Value = halId
                    });

                    Amazon.ECS.Model.ContainerDefinition containerDef = new Amazon.ECS.Model.ContainerDefinition
                    {
                        Cpu = cd.Cpu,
                        DisableNetworking = cd.DisableNetworking,
                        Environment = environmentVars,
                        DependsOn = cd.DependsOn?.Select(x => new Amazon.ECS.Model.ContainerDependency
                        {
                            Condition = x.Condition,
                            ContainerName = x.ContainerName
                        }).ToList(),
                        Image = cd.Image,
                        Memory = cd.Memory,
                        Name = cd.Name,
                        HealthCheck = cd.HealthCheck == null ? null : new()
                        {
                            Command = cd.HealthCheck.Command.ToList(),
                            Interval = cd.HealthCheck.Interval,
                            Retries = cd.HealthCheck.Retries,
                            Timeout = cd.HealthCheck.Timeout,
                            StartPeriod = cd.HealthCheck.StartPeriod
                        },
                        LogConfiguration = new Amazon.ECS.Model.LogConfiguration
                        {
                            LogDriver = cd.LogConfiguration?.LogDriver,
                            Options = new Dictionary<string, string>
                            {
                                { "awslogs-group", awsLogsGroup },
                                { "awslogs-stream-prefix", cd.LogConfiguration.Options.AwslogsStreamPrefix },
                                { "awslogs-region", cd.LogConfiguration.Options.AwslogsRegion },
                                { "awslogs-create-group", cd.LogConfiguration.Options.AwslogsCreateGroup }
                            }
                        },
                        LinuxParameters = new Amazon.ECS.Model.LinuxParameters
                        {
                            InitProcessEnabled = cd.LinuxParameters.InitProcessEnabled
                        },
                        VolumesFrom = cd.VolumesFrom?.Select(x => new Amazon.ECS.Model.VolumeFrom
                        {
                            ReadOnly = x.ReadOnly,
                            SourceContainer = x.SourceContainer
                        }).ToList(),
                        PortMappings = cd.PortMappings?.Select(x => new Amazon.ECS.Model.PortMapping
                        {
                            ContainerPort = x.ContainerPort,
                            HostPort = x.HostPort
                        }).ToList(),
                        Privileged = cd.Privileged
                    };

                    if (cd.LinuxParameters.SharedMemorySize > 0)
                    {
                        containerDef.LinuxParameters.SharedMemorySize = cd.LinuxParameters.SharedMemorySize;
                    }

                    return containerDef;
                }).ToList(),
                Cpu = configuration.EcsGridTaskDefinitionConfig.Cpu,
                ExecutionRoleArn = configuration.EcsGridTaskDefinitionConfig.ExecutionRoleArn,
                Family = gridTaskDefinition,
                Memory = configuration.EcsGridTaskDefinitionConfig.Memory,
                NetworkMode = configuration.EcsGridTaskDefinitionConfig.NetworkMode,
                RequiresCompatibilities = configuration.EcsGridTaskDefinitionConfig.RequiresCompatibilities.ToList(),
                TaskRoleArn = configuration.EcsGridTaskDefinitionConfig.TaskRoleArn
            };

            return await _awsElasticContainerService.RegisterTaskDefinitionAsync(request, ct);
        }

        public async Task<RegisterTaskDefinitionResponse> RegisterProxyTaskDefinitionInAwsAsync(string proxyTaskDefinition, string halId, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            RegisterTaskDefinitionRequest request = new RegisterTaskDefinitionRequest
            {
                ContainerDefinitions = configuration.EcsProxyTaskDefinitionConfig.ContainerDefinitions?.Select(cd =>
                {
                    string awsLogsGroup = cd.LogConfiguration.Options.AwslogsGroup.Replace("{halId}", halId);

                    List<Amazon.ECS.Model.KeyValuePair> environmentVars = new List<Amazon.ECS.Model.KeyValuePair>();
                    if (cd.Environment != null)
                    {
                        environmentVars = cd.Environment.Select(y => new Amazon.ECS.Model.KeyValuePair
                        {
                            Name = y.Name,
                            Value = y.Value
                        }).ToList();
                    }

                    environmentVars.Add(new Amazon.ECS.Model.KeyValuePair
                    {
                        Name = "HAL_ID",
                        Value = halId
                    });

                    Amazon.ECS.Model.ContainerDefinition containerDef = new Amazon.ECS.Model.ContainerDefinition
                    {
                        Cpu = cd.Cpu,
                        DisableNetworking = cd.DisableNetworking,
                        Environment = environmentVars,
                        DependsOn = cd.DependsOn?.Select(x => new Amazon.ECS.Model.ContainerDependency
                        {
                            Condition = x.Condition,
                            ContainerName = x.ContainerName
                        }).ToList(),
                        Image = cd.Image,
                        Memory = cd.Memory,
                        Name = cd.Name,
                        HealthCheck = cd.HealthCheck == null ? null : new()
                        {
                            Command = cd.HealthCheck.Command.ToList(),
                            Interval = cd.HealthCheck.Interval,
                            Retries = cd.HealthCheck.Retries,
                            Timeout = cd.HealthCheck.Timeout,
                            StartPeriod = cd.HealthCheck.StartPeriod
                        },
                        LogConfiguration = new Amazon.ECS.Model.LogConfiguration
                        {
                            LogDriver = cd.LogConfiguration?.LogDriver,
                            Options = new Dictionary<string, string>
                            {
                                { "awslogs-group", awsLogsGroup },
                                { "awslogs-stream-prefix", cd.LogConfiguration.Options.AwslogsStreamPrefix },
                                { "awslogs-region", cd.LogConfiguration.Options.AwslogsRegion },
                                { "awslogs-create-group", cd.LogConfiguration.Options.AwslogsCreateGroup }
                            }
                        },
                        LinuxParameters = new Amazon.ECS.Model.LinuxParameters
                        {
                            InitProcessEnabled = cd.LinuxParameters.InitProcessEnabled
                        },
                        VolumesFrom = cd.VolumesFrom?.Select(x => new Amazon.ECS.Model.VolumeFrom
                        {
                            ReadOnly = x.ReadOnly,
                            SourceContainer = x.SourceContainer
                        }).ToList(),
                        PortMappings = cd.PortMappings?.Select(x => new Amazon.ECS.Model.PortMapping
                        {
                            ContainerPort = x.ContainerPort,
                            HostPort = x.HostPort
                        }).ToList(),
                        Privileged = cd.Privileged
                    };

                    if (cd.LinuxParameters.SharedMemorySize > 0)
                    {
                        containerDef.LinuxParameters.SharedMemorySize = cd.LinuxParameters.SharedMemorySize;
                    }

                    return containerDef;
                }).ToList(),
                Cpu = configuration.EcsProxyTaskDefinitionConfig.Cpu,
                ExecutionRoleArn = configuration.EcsProxyTaskDefinitionConfig.ExecutionRoleArn,
                Family = proxyTaskDefinition,
                Memory = configuration.EcsProxyTaskDefinitionConfig.Memory,
                NetworkMode = configuration.EcsProxyTaskDefinitionConfig.NetworkMode,
                RequiresCompatibilities = configuration.EcsProxyTaskDefinitionConfig.RequiresCompatibilities.ToList(),
                TaskRoleArn = configuration.EcsProxyTaskDefinitionConfig.TaskRoleArn
            };

            return await _awsElasticContainerService.RegisterTaskDefinitionAsync(request, ct);
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
            _logger.LogInformation("Ensuring ECS service tasks are running.");
            Stopwatch mainStopWatch = new Stopwatch();
            Stopwatch intervalStopWatch = new Stopwatch();
            intervalStopWatch.Start();
            mainStopWatch.Start();
            bool ecsServiceTasksRunning = false;
            _logger.LogInformation("The wait time for ECS service tasks to be running is {DefaultTimeToWaitForEcsServicePendingTasks_InSeconds}", DefaultTimeToWaitForEcsServicePendingTasks_InSeconds);
            while (mainStopWatch.Elapsed.TotalSeconds <= DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
            {
                // Check elapsed time w/o stopping/resetting the stopwatch                
                if (intervalStopWatch.Elapsed.TotalSeconds >= 20)
                {
                    double timeout = (DefaultTimeToWaitForEcsServicePendingTasks_InSeconds - mainStopWatch.Elapsed.TotalSeconds);
                    _logger.LogInformation("Checking if ECS service tasks are running... Times out in: {timeout}", timeout);
                    // At least 20 seconds elapsed, restart stopwatch.
                    intervalStopWatch.Stop();
                    bool areECSServiceTasksRunning = await AreEcsServiceTasksRunningAsync(ecsServiceName, clusterArn, ct);
                    if (areECSServiceTasksRunning == true)
                    {
                        _logger.LogInformation("Successfully verified ecs service tasks are running");
                        mainStopWatch.Stop();
                        intervalStopWatch.Stop();
                        ecsServiceTasksRunning = true;
                        break;
                    }
                    else
                    {
                        _logger.LogInformation("ECS Service tasks are not running yet. Checking again in 20 seconds...");
                    }
                    intervalStopWatch.Restart();
                }

                if (mainStopWatch.Elapsed.TotalSeconds == DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
                {
                    _logger.LogWarning("ECS Service tasks were not started in the allotted time. Exiting...");
                    mainStopWatch.Stop();
                    intervalStopWatch.Stop();
                    ecsServiceTasksRunning = false;
                    break;
                }
            }

            _logger.LogInformation("Are ECS Service tasks running: {ecsServiceTasksRunning}", ecsServiceTasksRunning);
            return ecsServiceTasksRunning;
        }

        private async Task<bool> AreEcsServiceTasksRunningAsync(string ecsServiceName, string clusternArn, CancellationToken ct = default)
        {
            Amazon.ECS.Model.DescribeServicesResponse response = await DescribeServicesAsync(ecsServiceName, clusternArn, ct);

            if (response == null || response.Services == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("AWS request to figure out if ECS Service tasks are running has failed");
                return false;
            }

            if (response.Services.Count() > 1)
            {
                _logger.LogWarning("Expected to find a single service in the response but more than one were found. Using first service in the collection to check if all tasks are running");
            }

            return response.Services.First().PendingCount == 0 && response.Services.First().RunningCount > 0;
        }

        public async Task<VirtualAssistant> GetVirtualAssistantAsync(string userId, CancellationToken ct = default)
        {
            IList<VirtualAssistant> virtualAssistants = await _virtualAssistantRepository.GetAllByUserIdAsync(userId, ct);

            // for now we only expect users to have only one assistant.
            return virtualAssistants.FirstOrDefault();
        }

        public async Task<VirtualAssistant> CreateVirtualAssistantAsync(
            IList<EcsTaskDefinition> newEcsTaskDefinitions,
            IList<EcsService> newEcsServices,
            IList<CloudMapDiscoveryService> newCloudMapServices,
            string halId,
            ApplicationUser user,
            string timezoneId,
            CancellationToken ct = default)
        {
            HalUnit newHalUnit = new()
            {
                HalId = halId,
                TimeZoneId = timezoneId,
                ApplicationUser = user
            };

            VirtualAssistant virtualAssistant = new VirtualAssistant
            {
                EcsTaskDefinitions = newEcsTaskDefinitions,
                EcsServices = newEcsServices,
                CloudMapDiscoveryServices = newCloudMapServices,
                ApplicationUserId = user.Id,
                HalUnit = newHalUnit,
                HalId = halId,
            };

            return await _virtualAssistantRepository.CreateAsync(virtualAssistant, ct);
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
