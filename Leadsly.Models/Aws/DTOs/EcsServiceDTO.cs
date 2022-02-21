using Amazon.ECS;
using Leadsly.Models.Aws;
using Leadsly.Models.Aws.ElasticContainerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.DTOs
{
    public class EcsServiceDTO
    {
        public string UserId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceArn { get; set; }
        public List<EcsServiceRegistryDTO> Registries { get; set; }
        public string ClusterArn { get; set; }
        public string LaunchType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string TaskDefinition { get; set; }
        public int DesiredCount { get; set; }        
        public string AssignPublicIp { get; set; }
        public List<string> Subnets { get; set; }
        public List<string> SecurityGroups { get; set; }
        public string SchedulingStrategy { get; set; }
    }
}
