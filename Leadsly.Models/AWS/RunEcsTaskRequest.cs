using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws
{
    [DataContract]
    public class RunEcsTaskRequest
    {
        [DataMember(Name = "launchType", IsRequired = true)]
        public string LaunchType { get; set; }
        
        [DataMember(Name = "taskDefinition", IsRequired = true)]
        public string TaskDefinition { get; set; }
        [DataMember(IsRequired = true)]
        public string Cluster { get; set; }
        [DataMember(IsRequired = true)]
        public int Count { get; set; }

        [DataMember(Name = "assignPublicIp", IsRequired = false)]
        public string AssignPublicIp { get; set; }
        [DataMember(Name = "securityGroups", IsRequired = false)]
        public List<string> SecurityGroups { get; set; }
        [DataMember(Name = "subnets", IsRequired = true)]
        public List<string> Subnets { get; set; }
    }

    
}
