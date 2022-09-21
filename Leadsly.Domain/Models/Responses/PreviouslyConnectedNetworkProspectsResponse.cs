using Leadsly.Domain.Models.MonitorForNewConnections;
using System.Collections.Generic;

namespace Leadsly.Domain.Models.Responses
{
    public class PreviouslyConnectedNetworkProspectsResponse
    {
        public int PreviousTotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
