using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class TwoFactorAuthRequest
    {
        [DataMember(IsRequired = true)]
        public string Code { get; set; }

        [DataMember(IsRequired = true)]
        public string Username { get; set; }
    }
}
