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
        #region Campaign

        Task<Campaign> CreateAsync(Campaign newCampaign, CancellationToken ct = default);
        Task<Campaign> UpdateAsync(Campaign updatedCampaign, CancellationToken ct = default);        
        Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default);

        Task<IList<Campaign>> GetAllActiveByUserIdAsync(string applicationUserId, CancellationToken ct = default);
        Task<List<Campaign>> GetAllActiveAsync(CancellationToken ct = default);
        Task<List<Campaign>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default);

        Task<CampaignProspectList> GetCampaignProspectListByCampaignProspectListIdAsync(string campaignProspectListId, CancellationToken ct = default);

        Task<FollowUpMessage> GetFollowUpMessageByCampaignIdAsync(int order, string campaignId, CancellationToken ct = default);

        Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default);
        Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default);

        Task<IList<SendConnectionsStage>> GetSendConnectionStagesByIdAsync(string campaignId, CancellationToken ct = default);

        #endregion

        #region ProspectListPhase

        Task<ProspectListPhase> UpdateCampaignProspectListPhaseAsync(ProspectListPhase prospectListPhase, CancellationToken ct = default);
        Task<List<ProspectListPhase>> GetAllActivePropspectListPhasesAsync(CancellationToken ct = default);
        Task<ProspectListPhase> GetProspectListPhaseByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<ProspectListPhase> GetProspectListPhaseByPhaseIdAsync(string prospectListPhaseId, CancellationToken ct = default);
        Task<ProspectListPhase> GetProspectListPhaseByIdAsync(string campaignId, CancellationToken ct = default);

        #endregion

        #region MonitorForNewConnectionsPhase

        Task<IList<MonitorForNewConnectionsPhase>> GetAllMonitorForNewConnectionsPhasesByUserIdAsync(string userId, CancellationToken ct = default);
        Task<MonitorForNewConnectionsPhase> GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default);
        Task<MonitorForNewConnectionsPhase> CreateMonitorForNewConnectionsPhaseAsync(MonitorForNewConnectionsPhase phase, CancellationToken ct = default);

        #endregion

        #region ScanProspectsForReplies

        Task<ScanProspectsForRepliesPhase> CreateScanProspectsForRepliesPhaseAsync(ScanProspectsForRepliesPhase phase, CancellationToken ct = default);

        #endregion

        #region ConnectionWithdraw

        Task<ConnectionWithdrawPhase> CreateConnectionWithdrawPhaseAsync(ConnectionWithdrawPhase phase, CancellationToken ct = default);

        #endregion

        #region Prospects

        Task<PrimaryProspectList> GetPrimaryProspectListByIdAsync(string primaryProspectListId, CancellationToken ct = default);
        Task<PrimaryProspect> GetPrimaryProspectByIdAsync(string primaryProspectId, CancellationToken ct = default);
        Task<IList<PrimaryProspect>> CreatePrimaryProspectsAsync(IList<PrimaryProspect> primaryProspectList, CancellationToken ct = default);
        Task<IList<CampaignProspect>> CreateCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetCampaignProspectsByIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> UpdateCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);

        #endregion

        Task<SentConnectionsSearchUrlStatus> UpdateSentConnectionsStatusAsync(SentConnectionsSearchUrlStatus updatedSearchUrlStatus, CancellationToken ct = default);
        Task<IList<SentConnectionsSearchUrlStatus>> GetSentConnectionStatusesAsync(string campaignId, CancellationToken ct = default);
        Task<SentConnectionsSearchUrlStatus> GetSentConnectionStatusAsync(string campaignId, CancellationToken ct = default);        
        Task<string> GetChromeProfileNameByCampaignPhaseTypeAsync(PhaseType campaignType, CancellationToken ct = default);
        Task<ChromeProfileName> CreateChromeProfileNameAsync(ChromeProfileName chromeProfileName, CancellationToken ct = default);
    }
}
