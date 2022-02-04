using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class Enter2faAuthentiationCodeResultDTO
    {
        public bool Succeeded { get; set; }
        public bool CodeExpired { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool UnexpectedErrorOccured { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FailureDTO> Failures { get; set; }
    }
}
