using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
