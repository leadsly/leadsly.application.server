using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws
{
    [DataContract]
    public class RunTaskRequest
    {
        [DataMember(Name = "launchType", IsRequired = true)]
        public string LaunchType { get; set; }
        [DataMember(Name = "networkConfiguration", IsRequired = true)]
        public NetworkConfigurationDTO NetworkConfiguration { get; set; }
        [DataMember(Name = "taskDefinition", IsRequired = true)]
        public string TaskDefinition { get; set; }
    }

    
}
