using Leadsly.Domain.Models.MonitorForNewConnections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class UpdateCurrentConnectedNetworkProspectsRequest
    {
        [DataMember]
        public IList<RecentlyAddedProspectModel> Items { get; set; }

        [DataMember]
        public int PreviousTotalConnectionsCount { get; set; }
    }
}
