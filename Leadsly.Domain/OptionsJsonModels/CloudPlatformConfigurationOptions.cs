﻿using System.Collections.Generic;

namespace Leadsly.Domain.OptionsJsonModels
{
    public class CloudPlatformConfigurationOptions
    {
        public AwsOptions AwsOptions { get; set; }
    }

    public class AwsOptions
    {
        public string Region { get; set; }
        public string ApiServiceDiscoveryName { get; set; }
        public EcsServiceConfigOptions EcsServiceConfigOptions { get; set; }
        public EcsTaskConfigOptions EcsTaskConfigOptions { get; set; }
        public EcsTaskDefinitionOptions EcsTaskDefinitionConfigOptions { get; set; }
        public EcsTaskDefinitionOptions EcsGridTaskDefinitionConfigOptions { get; set; }
        public EcsTaskDefinitionOptions EcsHalTaskDefinitionConfigOptions { get; set; }
        public EcsServiceDiscoveryOptions EcsServiceDiscoveryConfigOptions { get; set; }
    }

    public class EcsContainerDefinition
    {
        public List<EcsPortMapping> PortMappings { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
    }

    public class EcsPortMapping
    {
        public int ContainerPort { get; set; }
        public string Protocol { get; set; }
    }

    public class EcsServiceDiscoveryOptions
    {
        public string NamespaceId { get; set; }
        public int DnsRecordTTL { get; set; }
        public string DnsRecordType { get; set; }
        public string Name { get; set; }
    }

    public class EcsServiceConfigOptions
    {
        public string AssignPublicIp { get; set; }
        public string ClusterArn { get; set; }
        public string ServiceName { get; set; }
        public int DesiredCount { get; set; }
        public List<string> Subnets { get; set; }
        public List<string> SecurityGroups { get; set; }
        public string LaunchType { get; set; }
        public string SchedulingStrategy { get; set; }
        public string TaskDefinition { get; set; }
    }

    public class EcsTaskConfigOptions
    {
        public string AssignPublicIp { get; set; }
        public string ClusterArn { get; set; }
        public int Count { get; set; }
        public string LaunchType { get; set; }
        public string TaskDefinition { get; set; }
        public List<string> Subnets { get; set; }
    }
}
