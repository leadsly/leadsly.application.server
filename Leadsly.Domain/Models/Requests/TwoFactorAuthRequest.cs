using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class TwoFactorAuthRequest
    {
        public string Code { get; set; }
    }
}
