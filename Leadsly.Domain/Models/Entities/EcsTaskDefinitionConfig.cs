namespace Leadsly.Domain.Models.Entities
{
    public class EcsTaskDefinitionConfig
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
        public bool Essential { get; set; }
        public string Image { get; set; }
        public int Memory { get; set; }
        public string Name { get; set; }
        public PortMapping[] PortMappings { get; set; }
        public bool Privileged { get; set; }
        public RepositoryCredentials RepositoryCredentials { get; set; }
        public int StartTimeout { get; set; }
        public int StopTimeout { get; set; }
        public VolumesFrom[] VolumesFrom { get; set; }
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
