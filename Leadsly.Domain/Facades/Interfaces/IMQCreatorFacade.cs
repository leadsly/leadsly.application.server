using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades.Interfaces
{
    public interface IMQCreatorFacade
    {
        Task PublishFollowUpMessagesMessageAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default);
        Task PublishMonitorForNewConnectionsMessageAsync(string halId, CancellationToken ct = default);
        Task PublishNetworkingMessageAsync(string halId, Campaign campaign, CancellationToken ct = default);
        Task PublishScanProspectsForRepliesMessageAsync(string halId, CancellationToken ct = default);
    }
}
