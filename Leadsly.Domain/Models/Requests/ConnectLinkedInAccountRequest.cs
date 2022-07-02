using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class ConnectLinkedInAccountRequest
    {
        [DataMember(Name = "username", IsRequired = true)]
        public string Username { get; set; } = string.Empty;
        [DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; } = string.Empty;
    }
}
