using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws
{
    [DataContract]
    public class CreateServiceRequest
    {
        [DataMember(IsRequired = true)]
        public string LaunchType { get; set; }

        [DataMember(IsRequired = true)]
        public string SchedulingStrategy { get; set; }

        [DataMember(IsRequired = true)]
        public string ServiceName { get; set; }

        [DataMember(IsRequired = false)]
        public List<Tag> Tags { get; set; }
    }    
}
