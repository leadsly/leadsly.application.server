using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class FailureDTO
    {
        [DataMember(EmitDefaultValue = false)]
        public string ResourceId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Arn { get; set; }
        [DataMember(IsRequired = true)]
        public string Detail { get; set; }
        [DataMember(IsRequired = true)]
        public string Reason { get; set; }
    }
}
