using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Models;

namespace Shared.Models.Leadsly
{
    [DataContract]
    public class UserSignoutFromAllDriversResultDTO
    {
        public bool Succeeded { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
        public List<FailureDTO> Failures { get; set; }
    }
}
