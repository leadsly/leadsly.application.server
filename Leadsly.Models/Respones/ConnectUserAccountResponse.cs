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
        public bool Succeeded { get; set; }
        public bool TwoFactorAuthRequired { get; set; }
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();
    }
}
