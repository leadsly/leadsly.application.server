using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class ConnectionsSentRequest
    {
        [DataMember]
        public IList<ConnectionSent> Items { get; set; }
    }
}
