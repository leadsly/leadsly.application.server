using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICampaignPhaseClient
    {
        Task HandleNewCampaignAsync(Campaign campaign);

        Task ProduceSendConnectionsPhaseAsync(string campaignId, string userId, CancellationToken ct = default);

        Task ProduceScanProspectsForRepliesPhaseAsync(string halId, string userId, CancellationToken ct = default);

        Task ProduceFollowUpMessagesPhaseAsync(string halId, string userId, CancellationToken ct = default);

        Task ProduceSendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
    }
}
