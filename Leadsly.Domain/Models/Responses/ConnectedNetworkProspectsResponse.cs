using Leadsly.Domain.Models.MonitorForNewConnections;
using System.Collections.Generic;

namespace Leadsly.Domain.Models.Responses
{
    public class ConnectedNetworkProspectsResponse
    {
        public int TotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
