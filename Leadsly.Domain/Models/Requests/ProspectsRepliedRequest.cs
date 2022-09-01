using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class ProspectsRepliedRequest
    {
        [DataMember]
        public IList<ProspectRepliedRequest> Items { get; set; }
    }
}
