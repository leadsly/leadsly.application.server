using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Leadsly
{
    [DataContract]
    public class UserSignoutFromAllDriversDTO
    {
        [DataMember(IsRequired = true)]
        public List<string> WebDriverIds { get; set; }
    }
}
