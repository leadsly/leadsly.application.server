using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Respones
{
    [DataContract]
    public class ConnectUserAccountResponse : LeadslyBaseResponse
    {
        [DataMember(IsRequired = true)]
        public bool Succeeded { get; set; }

        [DataMember(IsRequired = true)]
        public string WebDriverId { get; set; }
        [DataMember(IsRequired = true)]
        public bool TwoFactorAuthRequired { get; set; }
        [DataMember(IsRequired = true)]
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
        [DataMember(IsRequired = true)]
        public bool UnexpectedErrorOccured { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();
    }
}
