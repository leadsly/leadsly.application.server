using Amazon.ECS;
using Leadsly.Models.Aws;
using Leadsly.Models.Aws.ElasticContainerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class EcsServiceDTO
    {
        public string ServiceName { get; set; }
        public List<EcsServiceRegistryDTO> Registries { get; set; }
        public string ClusterArn { get; set; }
        public EcsLaunchType LaunchType { get; set; }
        public long CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public string TaskDefinition { get; set; }
        public int DesiredCount { get; set; }        
        public PublicIp AssignPublicIp { get; set; }
        public List<string> Subnets { get; set; }
        public string SchedulingStrategy { get; set; }
    }
}
