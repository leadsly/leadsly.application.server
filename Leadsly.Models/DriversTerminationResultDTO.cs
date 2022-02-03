using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class DriversTerminationResultDTO
    {
        public bool Succeeded { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long TerminationCompletedTimestamp { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FailureDTO> Failures { get; set; }
    }
}
