using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Models;

namespace Shared.Models.Leadsly.Requests
{
    [DataContract]
    public class New2faSmsCodeResultDTO
    {
        public bool Succeeded { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FailureDTO> Failures { get; set; }
        public long RequstedAtTimestamp { get; set; }
    }
}
