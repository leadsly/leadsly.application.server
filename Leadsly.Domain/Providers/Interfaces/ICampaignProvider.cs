using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICampaignProvider
    {
        Task<IList<ProspectListPhase>> GetIncompleteProspectListPhasesAsync(string halId, CancellationToken ct = default);
        Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default);
        Task<Campaign> UpdateCampaignAsync(Campaign campaign, CancellationToken ct = default);
        Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllByUserIdAsync(string userId, CancellationToken ct = default);
        Task<long> GetTotalConnectionsSentAsync(string campaignId, CancellationToken ct = default);
        Task<long> GetConnectionsAcceptedAsync(string campaignId, CancellationToken ct = default);
        Task<long> GetRepliesAsync(string campaignId, CancellationToken ct = default);
    }
}
