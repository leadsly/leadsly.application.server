using Amazon.ECS;
using Leadsly.Models.Aws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class ECSServiceDTO
    {
        public string Service { get; set; }        
        public string Cluster { get; set; }
        public EcsLaunchType LaunchType { get; set; }
        public long CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public string TaskDefinition { get; set; }
        public int DesiredCount { get; set; }
        public string RoleArn { get; set; }
        public PublicIp AssignPublicIp { get; set; }
        public string SchedulingStrategy { get; set; }
    }
}
