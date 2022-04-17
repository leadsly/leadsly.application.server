using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICampaignProvider
    {
        void ProcessNewCampaign(Campaign newCampaign);
        Task<HalsProspectListPhasesPayload> GetActiveProspectListPhasesAsync(CancellationToken ct = default);
        Task<HalsProspectListPhasesPayload> GetIncompleteProspectListPhasesAsync(CancellationToken ct = default);
        Task<List<string>> GetHalIdsWithActiveCampaignsAsync(CancellationToken ct = default);
        CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId);
        Task<int> CreateDailyWarmUpLimitConfigurationAsync(long startDateTimestamp, CancellationToken ct = default);
        void TriggerSendConnectionsPhase(string campaignId, string userId);

        void TriggerScanProspectsForRepliesPhase(string halId, string userId);
        void TriggerFollowUpMessagesPhase(string halId, string userId);
        Task SendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
    }
}
