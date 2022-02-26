using Leadsly.Models.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Respones.Hal
{
    [DataContract]
    public class ConnectAccountResponse : ResultBase, IConnectAccountResponse
    {
        [DataMember]
        public string WebDriverId { get; set; }
        [DataMember]
        public bool TwoFactorAuthRequired { get; set; }
        [DataMember]
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
        [DataMember]
        public bool UnexpectedErrorOccured { get; set; }
    }
}
