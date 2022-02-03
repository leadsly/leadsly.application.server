using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Leadsly.Requests
{
    [DataContract]
    public class DriversTerminationDTO
    {
        [DataMember(IsRequired = true)]
        public List<string> WebDriverIds { get; set; }

    }
}
