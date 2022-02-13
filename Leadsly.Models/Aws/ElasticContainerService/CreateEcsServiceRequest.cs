using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class CreateEcsServiceRequest
    {
        public string LaunchType { get; set; }

        public string SchedulingStrategy { get; set; }

        public string ServiceName { get; set; }

        public int DesiredCount { get; set; }
        
        public string TaskDefinition { get; set; }
        
        public string Cluster { get; set; }
        public List<string> Subnets { get; set; }
        public List<string> SecurityGroups { get; set; }
        public List<EcsServiceRegistry> EcsServiceRegistries { get; set; }

        public string AssignPublicIp { get; set; }
    }    
}
