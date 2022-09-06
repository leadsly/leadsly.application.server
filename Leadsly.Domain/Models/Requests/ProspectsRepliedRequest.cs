using Leadsly.Domain.Models.DeepScanProspectsForReplies;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class ProspectsRepliedRequest
    {
        [DataMember]
        public IList<ProspectRepliedModel> Items { get; set; }
    }
}
