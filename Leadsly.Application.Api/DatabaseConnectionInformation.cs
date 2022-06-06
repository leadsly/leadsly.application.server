using System.Runtime.Serialization;

namespace Leadsly.Application.Api
{
    [DataContract]
    public class DatabaseConnectionInformation
    {
        [DataMember(Name = "username")]
        public string UserName { get; set; }
        [DataMember(Name = "password")]
        public string Password { get; set; }
        [DataMember(Name = "engine")]
        public string Engine { get; set; }
        [DataMember(Name = "host")]
        public string Host { get; set; }
        [DataMember(Name = "port")]
        public string Port { get; set; }
        [DataMember(Name = "dbInstanceIdentifier")]
        public string DbInstanceIdentifier { get; set; }
    }
}
