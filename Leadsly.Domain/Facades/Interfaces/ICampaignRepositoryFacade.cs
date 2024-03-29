﻿using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades.Interfaces
{
    public interface ICampaignRepositoryFacade
    {
        #region Campaign

        Task<Campaign> CreateCampaignAsync(Campaign newCampaign, CancellationToken ct = default);
        Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign, CancellationToken ct = default);
        Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default);
        Task<bool> AnyActiveCampaignsByHalIdAsync(string halId, CancellationToken ct = default);
        Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllCampaignsByUserIdAsync(string userId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveCampaignsByHalIdAsync(string halId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveCampaignsAsync(CancellationToken ct = default);
        Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default);

        #endregion

        #region CampaignProspect

        Task<IList<CampaignProspect>> CreateAllCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllCampaignProspectsByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<CampaignProspect> GetCampaignProspectByProfileUrlAsync(string profileUrl, string halId, string userId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllActiveCampaignProspectsByHalIdAsync(string halId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllFollowUpMessageEligbleProspectsByCampaignIdAsync(string campaignId, CancellationToken ct);
        Task<CampaignProspect> GetCampaignProspectByIdAsync(string campaignProspectId, CancellationToken ct = default);
        Task<CampaignProspect> UpdateCampaignProspectAsync(CampaignProspect updatedCampaignProspect, CancellationToken ct = default);
        Task<IList<CampaignProspect>> UpdateAllCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);

        #endregion

        #region FollowUpMessagePhase

        Task<FollowUpMessagePhase> GetFollowUpMessagePhaseByCampaignIdAsync(string campaignId, CancellationToken ct = default);

        #endregion

        #region FollowUpMessages

        Task<IList<FollowUpMessage>> GetFollowUpMessagesByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<CampaignProspectFollowUpMessage> CreateFollowUpMessageAsync(CampaignProspectFollowUpMessage message, CancellationToken ct = default);
        Task<CampaignProspectFollowUpMessage> GetCampaignProspectFollowUpMessageByIdAsync(string campaignProspectFollowUpMessageId, CancellationToken ct = default);

        #endregion

        #region MonitorForNewConnectionsPhase

        Task<MonitorForNewConnectionsPhase> GetMonitorForNewConnectionsPhaseByIdAsync(string id, CancellationToken ct = default);
        Task<MonitorForNewConnectionsPhase> GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default);
        Task<bool> DeleteMonitorForNewConnectionsPhaseAsync(string monitorForNewConnectionsPhaseId, CancellationToken ct = default);

        #endregion

        #region PrimaryProspect

        Task<PrimaryProspectList> GetPrimaryProspectListByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default);
        Task<PrimaryProspectList> CreatePrimaryProspectListAsync(PrimaryProspectList primaryProspectList, CancellationToken ct = default);
        Task<PrimaryProspectList> GetPrimaryProspectListByIdAsync(string primaryProspectListId, CancellationToken ct = default);
        Task<IList<PrimaryProspect>> CreateAllPrimaryProspectsAsync(IList<PrimaryProspect> primaryProspectList, CancellationToken ct = default);

        #endregion

        #region ProspectListPhase
        Task<IList<ProspectListPhase>> GetAllActiveProspectListPhasesAsync(CancellationToken ct = default);
        Task<ProspectListPhase> GetProspectListPhaseByIdAsync(string prospectListPhaseId, CancellationToken ct = default);
        Task<bool> AnyIncompleteProspectListPhasesByHalIdAsync(string halId, CancellationToken ct = default);

        #endregion

        #region ScanProspectsForRepliesPhase
        Task<ScanProspectsForRepliesPhase> GetScanProspectsForRepliesPhaseByIdAsync(string scanProspectsForRepliesPhaseId, CancellationToken ct = default);
        Task<bool> DeleteScanProspectsForRepliesPhaseAsync(string scanProspectsForRepliesPhaseId, CancellationToken ct = default);

        #endregion

        #region SendConnectionsPhase
        Task<IList<SendConnectionsStage>> GetStagesByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        #endregion
    }
}
