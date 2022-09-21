using Leadsly.Domain.Models.MonitorForNewConnections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class UpdateConnectedNetworkProspectsRequest
    {
        [DataMember]
        public IList<RecentlyAddedProspectModel> Items { get; set; }

        [DataMember]
        public int TotalConnectionsCount { get; set; }
    }
}
