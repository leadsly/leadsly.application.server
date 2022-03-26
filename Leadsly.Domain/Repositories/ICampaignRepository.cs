using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ICampaignRepository
    {
        Task<List<Campaign>> GetAllActiveAsync(CancellationToken ct = default);

        Task<List<ProspectListPhase>> GetAllActivePropspectListPhasesAsync(CancellationToken ct = default);

        Task<List<Campaign>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default);

        Task<FollowUpMessage> GetFollowUpMessageByCampaignIdAsync(int order, string campaignId, CancellationToken ct = default);

        Task<Campaign> CreateAsync(Campaign newCampaign, CancellationToken ct = default);

        Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default);

        Task<ProspectListPhase> GetProspectListPhaseByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<PrimaryProspectList> GetProspectListByProspectListPhaseIdAsync(string prospectListPhaseId, CancellationToken ct = default);
        Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default);
        Task<ProspectListPhase> GetProspectListPhaseByIdAsync(string prospectListId, CancellationToken ct = default);
        Task<SentConnectionsStatus> CreateProspectListStatus(string campaignId, CancellationToken ct = default);
        Task<SentConnectionsStatus> GetSentConnectionStatusAsync(string campaignId, CancellationToken ct = default);
        Task<IEnumerable<CampaignProspect>> GetCampaignProspectsByIdAsync(string campaignId, CancellationToken ct = default);
    }
}
