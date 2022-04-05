using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Repositories;
using Leadsly.Application.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return await _dbContext.CloudMapServiceDiscoveryServices.AnyAsync(def => def.CloudMapServiceDiscoveryServiceId == id, ct);
        }

        public CloudPlatformConfiguration GetCloudPlatformConfiguration()
        {
            CloudPlatformConfiguration config = new()
            {
                Region = _cloudPlatformConfigurationOptions.AwsOptions.Region,
                ApiServiceDiscoveryName = _cloudPlatformConfigurationOptions.AwsOptions.ApiServiceDiscoveryName,
                EcsServiceConfig = new()
                {
                    AssignPublicIp = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.AssignPublicIp,
                    ClusterArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.ClusterArn,
                    LaunchType = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.LaunchType,
                    SecurityGroups = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.SecurityGroups,
                    DesiredCount = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.DesiredCount,
                    SchedulingStrategy = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.SchedulingStrategy,
                    ServiceName = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.ServiceName,
                    Subnets = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.Subnets,
                    TaskDefinition = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceConfigOptions.TaskDefinition
                },
                ServiceDiscoveryConfig = new()
                {
                    DnsRecordTTL = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.DnsRecordTTL,
                    DnsRecordType = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.DnsRecordType,
                    NamespaceId = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.NamespaceId,
                    Name = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.Name
                },
                EcsTaskDefinitionConfig = new()
                {
                    ContainerDefinitions = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.ContainerDefinitions.Select(c => new EcsContainerDefinitionConfig
                    {
                        Image = c.Image
                    }).ToList(),
                    Cpu = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.Cpu,
                    ExecutionRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.ExecutionRoleArn,
                    Memory = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.Memory,
                    NetworkMode = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.NetworkMode,
                    RequiresCompatibilities = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.RequiresCompatibilities,
                    TaskRoleArn = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.TaskRoleArn
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
            catch(Exception ex)
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
                _logger.LogError(ex, "Failed to save ecs task definition to the database");
                return null;
            }
            return newEcsService;
        }

        public async Task<CloudMapServiceDiscoveryService> AddServiceDiscoveryAsync(CloudMapServiceDiscoveryService newCloudMapServiceDiscovery, CancellationToken ct = default)
        {
            try
            {
                _dbContext.CloudMapServiceDiscoveryServices.Add(newCloudMapServiceDiscovery);
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

        public async Task<bool> RemoveCloudMapServiceDiscoveryServiceAsync(string discoveryServiceId, CancellationToken ct = default)
        {
            try
            {
                if (!await ServiceDiscoveryServiceExists(discoveryServiceId, ct))
                {
                    return false;
                }
                CloudMapServiceDiscoveryService toRemove = _dbContext.CloudMapServiceDiscoveryServices.Find(discoveryServiceId);
                _dbContext.CloudMapServiceDiscoveryServices.Remove(toRemove);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove service discovery service from the database");
                return false;
            }
            return true;
        }
    }
}
