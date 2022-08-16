using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class EmailChallengePinRequest
    {
        [DataMember(IsRequired = true)]
        public string Pin { get; set; }

        [DataMember(IsRequired = true)]
        public string Username { get; set; }
    }
}
