using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class AwsvpcConfiguration
    {
        /// <summary>
        /// Whether the task's elastic network interface receives a public IP address. The default value is DISABLED.
        /// Valid Values: ENABLED | DISABLED
        /// </summary>        
        [DataMember(Name = "assignPublicIp", IsRequired = false)]
        public string AssignPublicIp { get; set; }
        [DataMember(Name = "securityGroups", IsRequired = false)]
        public List<string> SecurityGroups { get; set; }
        [DataMember(Name = "subnets", IsRequired = true)]
        public List<string> Subnets { get; set; }
    }
}
