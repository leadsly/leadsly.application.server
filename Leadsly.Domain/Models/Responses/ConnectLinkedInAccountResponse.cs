using Leadsly.Application.Model;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Responses
{
    [DataContract]
    public class ConnectLinkedInAccountResponse
    {
        [DataMember(IsRequired = true)]
        public bool InvalidPassword { get; set; }

        [DataMember(IsRequired = true)]
        public bool InvalidEmail { get; set; }

        [DataMember(IsRequired = true)]
        public bool EmailPinChallenge { get; set; }

        [DataMember(IsRequired = true)]
        public bool TwoFactorAuthRequired { get; set; }

        [DataMember(IsRequired = true)]
        public TwoFactorAuthType TwoFactorAuthType { get; set; }

        [DataMember(IsRequired = true)]
        public bool UnexpectedErrorOccured { get; set; }
    }
}
