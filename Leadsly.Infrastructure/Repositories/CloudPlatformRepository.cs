using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class CloudPlatformRepository : ICloudPlatformRepository
    {
        public CloudPlatformRepository(DatabaseContext dbContext, ILogger<CloudPlatformRepository> logger, IOptions<CloudPlatformConfigurationOptions> cloudPlatformConfigurationOptions)
        {
            _cloudPlatformConfigurationOptions = cloudPlatformConfigurationOptions.Value;
            _dbContext = dbContext;
            _logger = logger;

        }

        private readonly CloudPlatformConfigurationOptions _cloudPlatformConfigurationOptions;
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<CloudPlatformRepository> _logger;

        private async Task<bool> EcsServiceExists(string id, CancellationToken ct = default)
        {
            return await _dbContext.EcsServices.AnyAsync(ser => ser.EcsServiceId == id, ct);
        }

        private async Task<bool> EcsTaskDefinitionExists(string id, CancellationToken ct = default)
        {
            return await _dbContext.EcsTaskDefinitions.AnyAsync(def => def.EcsTaskDefinitionId == id, ct);
        }

        private async Task<bool> ServiceDiscoveryServiceExists(string id, CancellationToken ct = default)
        {
            return await _dbContext.CloudMapDiscoveryServices.AnyAsync(def => def.CloudMapDiscoveryServiceId == id, ct);
        }

        public CloudPlatformConfiguration GetCloudPlatformConfiguration()
        {
            CloudPlatformConfiguration config = new()
            {
                Region = _cloudPlatformConfigurationOptions.AwsOptions.Region,
                ApiServiceDiscoveryName = _cloudPlatformConfigurationOptions.AwsOptions.ApiServiceDiscoveryName,
                EcsServiceConfig = new()
                {
                    Grid = new()
                    {
                        AssignPublicIp = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.AssignPublicIp,
                        ClusterArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.ClusterArn,
                        LaunchType = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.LaunchType,
                        SecurityGroups = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.SecurityGroups,
                        DesiredCount = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.DesiredCount,
                        SchedulingStrategy = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.SchedulingStrategy,
                        ServiceName = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.ServiceName,
                        Subnets = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.Subnets,
                        TaskDefinition = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Grid.TaskDefinition
                    },
                    Hal = new()
                    {
                        AssignPublicIp = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.AssignPublicIp,
                        ClusterArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.ClusterArn,
                        LaunchType = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.LaunchType,
                        SecurityGroups = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.SecurityGroups,
                        DesiredCount = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.DesiredCount,
                        SchedulingStrategy = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.SchedulingStrategy,
                        ServiceName = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.ServiceName,
                        Subnets = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.Subnets,
                        TaskDefinition = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Hal.TaskDefinition
                    }
                },
                ServiceDiscoveryConfig = new()
                {
                    AppServer = new()
                    {
                        DnsRecordTTL = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.AppServer.DnsRecordTTL,
                        DnsRecordType = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.AppServer.DnsRecordType,
                        NamespaceId = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.AppServer.NamespaceId,
                        Name = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.AppServer.Name
                    },
                    Grid = new()
                    {
                        DnsRecordTTL = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Grid.DnsRecordTTL,
                        DnsRecordType = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Grid.DnsRecordType,
                        NamespaceId = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Grid.NamespaceId,
                        Name = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Grid.Name
                    },
                    Hal = new()
                    {
                        DnsRecordTTL = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Hal.DnsRecordTTL,
                        DnsRecordType = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Hal.DnsRecordType,
                        NamespaceId = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Hal.NamespaceId,
                        Name = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Hal.Name
                    }
                },
                EcsProxyTaskDefinitionConfig = new()
                {
                    ContainerDefinitions = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.ContainerDefinitions?.Select(c => new Domain.Models.Entities.ContainerDefinition
                    {
                        Name = c.Name,
                        Image = c.Image,
                        Cpu = c.Cpu,
                        Memory = c.Memory,
                        Environment = c.Environment?.Select(e => new Domain.Models.Entities.Environment
                        {
                            Name = e.Name,
                            Value = e.Value
                        }).ToArray(),
                        LogConfiguration = new Domain.Models.Entities.LogConfiguration
                        {
                            LogDriver = c.LogConfiguration?.LogDriver,
                            Options = new Domain.Models.Entities.Options
                            {
                                AwslogsCreateGroup = c.LogConfiguration?.Options?.AwslogsCreateGroup,
                                AwslogsGroup = c.LogConfiguration?.Options?.AwslogsGroup,
                                AwslogsRegion = c.LogConfiguration?.Options?.AwslogsRegion,
                                AwslogsStreamPrefix = c.LogConfiguration?.Options?.AwslogsStreamPrefix
                            }
                        },
                        LinuxParameters = new Domain.Models.Entities.LinuxParameters
                        {
                            InitProcessEnabled = c.LinuxParameters.InitProcessEnabled,
                            SharedMemorySize = c.LinuxParameters.SharedMemorySize
                        },
                        PortMappings = c.PortMappings?.Select(p => new Domain.Models.Entities.PortMapping
                        {
                            ContainerPort = p.ContainerPort,
                            HostPort = p.HostPort
                        }).ToArray(),
                        DependsOn = c.DependsOn?.Select(d => new Domain.Models.Entities.DependsOn
                        {
                            ContainerName = d.ContainerName,
                            Condition = d.Condition
                        }).ToArray(),
                        DisableNetworking = c.DisableNetworking,
                        Essential = c.Essential,
                        HealthCheck = new()
                        {
                            Command = c.HealthCheck.Command,
                            Interval = c.HealthCheck.Interval,
                            Retries = c.HealthCheck.Retries,
                            Timeout = c.HealthCheck.Timeout,
                            StartPeriod = c.HealthCheck.StartPeriod
                        },
                        Privileged = c.Privileged,
                        VolumesFrom = c.VolumesFrom?.Select(v => new Domain.Models.Entities.VolumesFrom
                        {
                            SourceContainer = v.SourceContainer,
                            ReadOnly = v.ReadOnly
                        }).ToArray(),
                        StartTimeout = c.StartTimeout,
                        StopTimeout = c.StopTimeout
                    }).ToArray(),
                    Family = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.Family,
                    NetworkMode = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.NetworkMode,
                    RequiresCompatibilities = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.RequiresCompatibilities,
                    TaskRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.TaskRoleArn,
                    Cpu = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.Cpu,
                    Memory = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.Memory,
                    ExecutionRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsProxyTaskDefinitionConfigOptions.ExecutionRoleArn
                },
                EcsGridTaskDefinitionConfig = new()
                {
                    ContainerDefinitions = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.ContainerDefinitions?.Select(c => new Domain.Models.Entities.ContainerDefinition
                    {
                        Name = c.Name,
                        Image = c.Image,
                        Cpu = c.Cpu,
                        Memory = c.Memory,
                        Environment = c.Environment?.Select(e => new Domain.Models.Entities.Environment
                        {
                            Name = e.Name,
                            Value = e.Value
                        }).ToArray(),
                        LogConfiguration = new Domain.Models.Entities.LogConfiguration
                        {
                            LogDriver = c.LogConfiguration?.LogDriver,
                            Options = new Domain.Models.Entities.Options
                            {
                                AwslogsCreateGroup = c.LogConfiguration?.Options?.AwslogsCreateGroup,
                                AwslogsGroup = c.LogConfiguration?.Options?.AwslogsGroup,
                                AwslogsRegion = c.LogConfiguration?.Options?.AwslogsRegion,
                                AwslogsStreamPrefix = c.LogConfiguration?.Options?.AwslogsStreamPrefix
                            }
                        },
                        LinuxParameters = new Domain.Models.Entities.LinuxParameters
                        {
                            InitProcessEnabled = c.LinuxParameters.InitProcessEnabled,
                            SharedMemorySize = c.LinuxParameters.SharedMemorySize
                        },
                        PortMappings = c.PortMappings?.Select(p => new Domain.Models.Entities.PortMapping
                        {
                            ContainerPort = p.ContainerPort,
                            HostPort = p.HostPort
                        }).ToArray(),
                        DependsOn = c.DependsOn?.Select(d => new Domain.Models.Entities.DependsOn
                        {
                            ContainerName = d.ContainerName,
                            Condition = d.Condition
                        }).ToArray(),
                        DisableNetworking = c.DisableNetworking,
                        Essential = c.Essential,
                        HealthCheck = c.HealthCheck == null ? null : new()
                        {
                            Command = c.HealthCheck.Command,
                            Interval = c.HealthCheck.Interval,
                            Retries = c.HealthCheck.Retries,
                            Timeout = c.HealthCheck.Timeout,
                            StartPeriod = c.HealthCheck.StartPeriod
                        },
                        Privileged = c.Privileged,
                        VolumesFrom = c.VolumesFrom?.Select(v => new Domain.Models.Entities.VolumesFrom
                        {
                            SourceContainer = v.SourceContainer,
                            ReadOnly = v.ReadOnly
                        }).ToArray(),
                        StartTimeout = c.StartTimeout,
                        StopTimeout = c.StopTimeout
                    }).ToArray(),
                    Family = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.Family,
                    NetworkMode = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.NetworkMode,
                    RequiresCompatibilities = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.RequiresCompatibilities,
                    TaskRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.TaskRoleArn,
                    Cpu = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.Cpu,
                    Memory = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.Memory,
                    ExecutionRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsGridTaskDefinitionConfigOptions.ExecutionRoleArn
                },
                EcsHalTaskDefinitionConfig = new()
                {
                    ContainerDefinitions = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.ContainerDefinitions?.Select(c => new Domain.Models.Entities.ContainerDefinition
                    {
                        Name = c.Name,
                        Image = c.Image,
                        Cpu = c.Cpu,
                        Memory = c.Memory,
                        Environment = c.Environment?.Select(e => new Domain.Models.Entities.Environment
                        {
                            Name = e.Name,
                            Value = e.Value
                        }).ToArray(),
                        LogConfiguration = new Domain.Models.Entities.LogConfiguration
                        {
                            LogDriver = c.LogConfiguration?.LogDriver,
                            Options = new Domain.Models.Entities.Options
                            {
                                AwslogsCreateGroup = c.LogConfiguration?.Options?.AwslogsCreateGroup,
                                AwslogsGroup = c.LogConfiguration?.Options?.AwslogsGroup,
                                AwslogsRegion = c.LogConfiguration?.Options?.AwslogsRegion,
                                AwslogsStreamPrefix = c.LogConfiguration?.Options?.AwslogsStreamPrefix
                            }
                        },
                        LinuxParameters = new Domain.Models.Entities.LinuxParameters
                        {
                            InitProcessEnabled = c.LinuxParameters.InitProcessEnabled,
                            SharedMemorySize = c.LinuxParameters.SharedMemorySize
                        },
                        PortMappings = c.PortMappings?.Select(p => new Domain.Models.Entities.PortMapping
                        {
                            ContainerPort = p.ContainerPort,
                            HostPort = p.HostPort
                        }).ToArray(),
                        DependsOn = c.DependsOn?.Select(d => new Domain.Models.Entities.DependsOn
                        {
                            ContainerName = d.ContainerName,
                            Condition = d.Condition
                        }).ToArray(),
                        DisableNetworking = c.DisableNetworking,
                        Essential = c.Essential,
                        HealthCheck = new()
                        {
                            Command = c.HealthCheck.Command,
                            Interval = c.HealthCheck.Interval,
                            Retries = c.HealthCheck.Retries,
                            Timeout = c.HealthCheck.Timeout,
                            StartPeriod = c.HealthCheck.StartPeriod
                        },
                        Privileged = c.Privileged,
                        VolumesFrom = c.VolumesFrom?.Select(v => new Domain.Models.Entities.VolumesFrom
                        {
                            SourceContainer = v.SourceContainer,
                            ReadOnly = v.ReadOnly
                        }).ToArray(),
                        StartTimeout = c.StartTimeout,
                        StopTimeout = c.StopTimeout
                    }).ToArray(),
                    Family = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.Family,
                    NetworkMode = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.NetworkMode,
                    RequiresCompatibilities = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.RequiresCompatibilities,
                    TaskRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.TaskRoleArn,
                    Cpu = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.Cpu,
                    Memory = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.Memory,
                    ExecutionRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsHalTaskDefinitionConfigOptions.ExecutionRoleArn
                }
            };

            return config;
        }

        public async Task<EcsTaskDefinition> AddEcsTaskDefinitionAsync(EcsTaskDefinition newEcsTaskDefinition, CancellationToken ct = default)
        {
            try
            {
                _dbContext.EcsTaskDefinitions.Add(newEcsTaskDefinition);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save ecs task definition to the database");
                return null;
            }
            return newEcsTaskDefinition;
        }

        public async Task<EcsService> AddEcsServiceAsync(EcsService newEcsService, CancellationToken ct = default)
        {
            try
            {
                _dbContext.EcsServices.Add(newEcsService);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save ecs service to the database");
                return null;
            }
            return newEcsService;
        }

        public async Task<EcsService> UpdateEcsServiceAsync(EcsService updatedEcsService, CancellationToken ct = default)
        {
            try
            {
                _dbContext.EcsServices.Update(updatedEcsService);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update ecs service in the database");
                return null;
            }
            return updatedEcsService;
        }

        public async Task<IList<EcsTask>> UpdateEcsTasksAsync(IList<EcsTask> updatedEcsTasks, CancellationToken ct = default)
        {
            try
            {
                _dbContext.EcsTasks.UpdateRange(updatedEcsTasks);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update ecs tasks in the database");
                return null;
            }
            return updatedEcsTasks;
        }

        public async Task<CloudMapDiscoveryService> AddServiceDiscoveryAsync(CloudMapDiscoveryService newCloudMapServiceDiscovery, CancellationToken ct = default)
        {
            try
            {
                _dbContext.CloudMapDiscoveryServices.Add(newCloudMapServiceDiscovery);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save ecs task definition to the database");
                return null;
            }
            return newCloudMapServiceDiscovery;
        }

        public async Task<bool> RemoveEcsTaskDefinitionAsync(string ecsTaskDefinitionId, CancellationToken ct = default)
        {
            try
            {
                if (!await EcsTaskDefinitionExists(ecsTaskDefinitionId, ct))
                {
                    return false;
                }
                EcsTaskDefinition toRemove = _dbContext.EcsTaskDefinitions.Find(ecsTaskDefinitionId);
                _dbContext.EcsTaskDefinitions.Remove(toRemove);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove ecs task definition from the database");
                return false;
            }
            return true;
        }

        public async Task<bool> RemoveEcsServiceAsync(string ecsServiceId, CancellationToken ct = default)
        {
            try
            {
                if (!await EcsServiceExists(ecsServiceId, ct))
                {
                    return false;
                }
                EcsService toRemove = _dbContext.EcsServices.Find(ecsServiceId);
                _dbContext.EcsServices.Remove(toRemove);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove ecs service from the database");
                return false;
            }
            return true;
        }

        public async Task<bool> RemoveEcsTasksByServiceIdAsync(string ecsServiceId, CancellationToken ct = default)
        {
            try
            {
                IList<EcsTask> ecsTasks = await _dbContext.EcsTasks.Where(t => t.EcsServiceId == ecsServiceId).ToListAsync(ct);
                _dbContext.EcsTasks.RemoveRange(ecsTasks);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove ECS Tasks by ECS service id: {ecsServiceId}", ecsServiceId);
                return false;
            }

            return true;
        }

        public async Task<bool> RemoveCloudMapServiceDiscoveryServiceAsync(string discoveryServiceId, CancellationToken ct = default)
        {
            try
            {
                if (!await ServiceDiscoveryServiceExists(discoveryServiceId, ct))
                {
                    return false;
                }
                CloudMapDiscoveryService toRemove = _dbContext.CloudMapDiscoveryServices.Find(discoveryServiceId);
                _dbContext.CloudMapDiscoveryServices.Remove(toRemove);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove service discovery service from the database");
                return false;
            }
            return true;
        }

        public async Task<VirtualAssistant> CreateVirtualAssistantAsync(VirtualAssistant newVirtualAssistant, CancellationToken ct = default)
        {
            try
            {
                _dbContext.VirtualAssistants.Add(newVirtualAssistant);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add new virtual assistant");
            }
            return newVirtualAssistant;
        }
    }
}
