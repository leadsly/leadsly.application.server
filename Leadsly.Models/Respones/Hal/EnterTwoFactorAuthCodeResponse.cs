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
    public class EnterTwoFactorAuthCodeResponse : ResultBase, IEnterTwoFactorAuthCodeResponse
    {
        [DataMember]
        public bool InvalidOrExpiredCode { get; set; }
        [DataMember]
        public bool DidUnexpectedErrorOccur { get; set; }
    }
}
