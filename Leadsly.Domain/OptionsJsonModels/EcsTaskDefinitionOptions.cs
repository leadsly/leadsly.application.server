﻿namespace Leadsly.Domain.OptionsJsonModels
{

    public class EcsTaskDefinitionOptions
    {
        public ContainerDefinition[] ContainerDefinitions { get; set; }
        public string Cpu { get; set; }
        public string ExecutionRoleArn { get; set; }
        public string Family { get; set; }
        public string Memory { get; set; }
        public string NetworkMode { get; set; }
        public string[] RequiresCompatibilities { get; set; }
        public string TaskRoleArn { get; set; }
    }

    public class ContainerDefinition
    {
        public int Cpu { get; set; }
        public DependsOn[] DependsOn { get; set; }
        public bool DisableNetworking { get; set; }
        public Environment[] Environment { get; set; }
        public LinuxParameters LinuxParameters { get; set; }
        public HealthCheck HealthCheck { get; set; }
        public bool Essential { get; set; }
        public string Image { get; set; }
        public int Memory { get; set; }
        public string Name { get; set; }
        public PortMapping[] PortMappings { get; set; }
        public bool Privileged { get; set; }
        public RepositoryCredentials RepositoryCredentials { get; set; }
        public int StartTimeout { get; set; }
        public LogConfiguration LogConfiguration { get; set; }
        public int StopTimeout { get; set; }
        public VolumesFrom[] VolumesFrom { get; set; }
    }

    public class HealthCheck
    {
        public string[] Command { get; set; }
        public int Interval { get; set; }
        public int Retries { get; set; }
        public int StartPeriod { get; set; }
        public int Timeout { get; set; }
    }

    public class LogConfiguration
    {
        public string LogDriver { get; set; }
        public Options Options { get; set; }
    }

    public class Options
    {
        public string AwslogsCreateGroup { get; set; }
        public string AwslogsGroup { get; set; }
        public string AwslogsRegion { get; set; }
        public string AwslogsStreamPrefix { get; set; }
    }

    public class LinuxParameters
    {
        public bool InitProcessEnabled { get; set; }
        public int SharedMemorySize { get; set; }
    }

    public class RepositoryCredentials
    {
        public string CredentialsParameter { get; set; }
    }

    public class DependsOn
    {
        public string Condition { get; set; }
        public string ContainerName { get; set; }
    }

    public class Environment
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class PortMapping
    {
        public int ContainerPort { get; set; }
        public int HostPort { get; set; }
    }

    public class VolumesFrom
    {
        public bool ReadOnly { get; set; }
        public string SourceContainer { get; set; }
    }
}
