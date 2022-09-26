using System.Collections.Generic;

namespace Leadsly.Domain.Models.Entities
{
    public class EcsServiceConfig
    {
        public Config Hal { get; set; }
        public Config Grid { get; set; }
        public Config Proxy { get; set; }
    }

    public class Config
    {
        public string AssignPublicIp { get; set; }
        public string ClusterArn { get; set; }
        public int DesiredCount { get; set; }
        public List<string> Subnets { get; set; }
        public List<string> SecurityGroups { get; set; }
        public string SchedulingStrategy { get; set; }
        public string LaunchType { get; set; }
        public string ServiceName { get; set; }
        public string TaskDefinition { get; set; }
    }
}
