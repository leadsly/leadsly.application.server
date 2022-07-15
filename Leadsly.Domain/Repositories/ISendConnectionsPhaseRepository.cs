using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ISendConnectionsPhaseRepository
    {
        Task<SearchUrlDetails> UpdateSentConnectionsStatusAsync(SearchUrlDetails updatedSearchUrlStatus, CancellationToken ct = default);
        Task<IList<SearchUrlDetails>> GetAllSentConnectionsStatusesAsync(string campaignId, CancellationToken ct = default);
        Task<IList<SendConnectionsStage>> GetStagesByCampaignIdAsync(string campaignId, CancellationToken ct = default);

    }
}
