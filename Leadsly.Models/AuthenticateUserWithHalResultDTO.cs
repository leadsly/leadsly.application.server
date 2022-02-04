using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class AuthenticateUserWithHalResultDTO
    {
        [DataMember(IsRequired = true)]
        public bool Succeeded { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FailureDTO> Failures { get; set; }
        [DataMember(IsRequired = true)]
        public bool TwoFactorAuthRequired { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
    }
}
