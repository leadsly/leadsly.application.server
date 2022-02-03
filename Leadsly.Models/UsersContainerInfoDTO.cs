using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Models;

namespace Shared.Models.Leadsly.s
{
    [DataContract]
    public class UsersContainerInfoDTO
    {
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public bool Succeeded { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FailureDTO> Failures { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string PrivateIpV6 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PrivateIpv4 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PublicIpv6 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PublicIpv4 { get; set; }
    }
}
