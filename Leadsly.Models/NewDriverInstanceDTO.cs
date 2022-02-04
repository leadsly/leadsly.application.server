using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Models;

namespace Shared.Models.Leadsly.Requests
{
    [DataContract]
    public class NewDriverInstanceRequestDTO
    {
        [DataMember(IsRequired = true)]
        public NewDriverInstancePurposeDTO Purpose { get; set; }

    }
}
