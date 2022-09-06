using Leadsly.Domain.Models.MonitorForNewConnections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class RecentlyAddedProspectsRequest
    {
        [DataMember]
        public string ApplicationUserId { get; set; }
        [DataMember]
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
