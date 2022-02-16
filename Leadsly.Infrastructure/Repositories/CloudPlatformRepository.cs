using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Repositories;
using Leadsly.Models.Entities;
using Microsoft.Extensions.Configuration;
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
        public CloudPlatformRepository(IOptions<CloudPlatformConfigurationOptions> cloudPlatformConfigurationOptions)
        {
            _cloudPlatformConfigurationOptions = cloudPlatformConfigurationOptions.Value;
        }

        private readonly CloudPlatformConfigurationOptions _cloudPlatformConfigurationOptions;

        public CloudPlatformConfiguration GetCloudPlatformConfiguration()
        {
            CloudPlatformConfiguration config = new()
            {
                Region = _cloudPlatformConfigurationOptions.AwsOptions.Region,
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
                    NamespaceId = _cloudPlatformConfigurationOptions.AwsOptions.EcsServiceDiscoveryConfigOptions.NamespaceId
                },
                EcsTaskDefinitionConfig = new()
                {
                    ContainerDefinitions = _cloudPlatformConfigurationOptions.AwsOptions.EcsTaskDefinitionConfigOptions.ContainerDefinitions.Select(c => new EcsContainerDefinitionConfig
                    {
                        Image = c.Image,
                        PortMappings = c.PortMappings.Select(p => new EcsPortMappingConfig
                        {
                            ContainerPort = p.ContainerPort,
                            Protocol = p.Protocol
                        }).ToList()
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
    }
}
