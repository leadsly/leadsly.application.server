using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades
{
    public class CampaignRepositoryFacade : ICampaignRepositoryFacade
    {
        public CampaignRepositoryFacade(
            ICampaignProspectRepository campaignProspectRepository,
            ICampaignRepository campaignRepository,
            IConnectionWithdrawPhaseRepository connectionWithdrawPhaseRepository,
            IFollowUpMessagePhaseRepository followUpMessagePhaseRepository,
            IMonitorForNewConnectionsPhaseRepository monitorForNewConnectionsPhaseRepository,
            IPrimaryProspectRepository primaryProspectRepository,
            IProspectListPhaseRepository prospectListPhaseRepository,
            IScanProspectsForRepliesPhaseRepository scanProspectsForRepliesPhaseRepository,
            ISendConnectionsPhaseRepository sendConnectionsPhaseRepository,
            IFollowUpMessageRepository followUpMessageRepository
            )
        {
            _campaignProspectRepository = campaignProspectRepository;
            _campaignRepository = campaignRepository;
            _followUpMessageRepository = followUpMessageRepository;
            _connectionWithdrawPhaseRepository = connectionWithdrawPhaseRepository;
            _followUpMessagePhaseRepository = followUpMessagePhaseRepository;
            _monitorForNewConnectionsPhaseRepository = monitorForNewConnectionsPhaseRepository;
            _primaryProspectRepository = primaryProspectRepository;
            _prospectListPhaseRepository = prospectListPhaseRepository;
            _scanProspectsForRepliesPhaseRepository = scanProspectsForRepliesPhaseRepository;
            _sendConnectionsPhaseRepository = sendConnectionsPhaseRepository;
        }

        private readonly ICampaignProspectRepository _campaignProspectRepository;
        private readonly IFollowUpMessageRepository _followUpMessageRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IConnectionWithdrawPhaseRepository _connectionWithdrawPhaseRepository;
        private readonly IFollowUpMessagePhaseRepository _followUpMessagePhaseRepository;
        private readonly IMonitorForNewConnectionsPhaseRepository _monitorForNewConnectionsPhaseRepository;
        private readonly IPrimaryProspectRepository _primaryProspectRepository;
        private readonly IProspectListPhaseRepository _prospectListPhaseRepository;
        private readonly IScanProspectsForRepliesPhaseRepository _scanProspectsForRepliesPhaseRepository;
        private readonly ISendConnectionsPhaseRepository _sendConnectionsPhaseRepository;

        public async Task<IList<CampaignProspect>> CreateAllCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.CreateAllAsync(campaignProspects, ct);
        }

        public async Task<IList<PrimaryProspect>> CreateAllPrimaryProspectsAsync(IList<PrimaryProspect> primaryProspectList, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.CreateAllAsync(primaryProspectList, ct);
        }

        public async Task<Campaign> CreateCampaignAsync(Campaign newCampaign, CancellationToken ct = default)
        {
            return await _campaignRepository.CreateAsync(newCampaign, ct);
        }

        public async Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default)
        {
            return await _campaignRepository.CreateCampaignWarmUpAsync(warmUp, ct);
        }

        public async Task<ConnectionWithdrawPhase> CreateConnectionWithdrawPhaseAsync(ConnectionWithdrawPhase phase, CancellationToken ct = default)
        {
            return await _connectionWithdrawPhaseRepository.CreateAsync(phase, ct);
        }

        public async Task<MonitorForNewConnectionsPhase> CreateMonitorForNewConnectionsPhaseAsync(MonitorForNewConnectionsPhase phase, CancellationToken ct = default)
        {
            return await _monitorForNewConnectionsPhaseRepository.CreateAsync(phase, ct);
        }

        public async Task<PrimaryProspectList> CreatePrimaryProspectListAsync(PrimaryProspectList primaryProspectList, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.CreateListAsync(primaryProspectList, ct);
        }

        public async Task<ScanProspectsForRepliesPhase> CreateScanProspectsForRepliesPhaseAsync(ScanProspectsForRepliesPhase phase, CancellationToken ct = default)
        {
            return await _scanProspectsForRepliesPhaseRepository.CreateAsync(phase, ct);
        }

        public async Task<IList<Campaign>> GetAllActiveCampaignsAsync(CancellationToken ct = default)
        {
            return await _campaignRepository.GetAllActiveAsync(ct);
        }

        public async Task<IList<Campaign>> GetAllActiveCampaignsByUserIdAsync(string applicationUserId, CancellationToken ct = default)
        {
            return await _campaignRepository.GetAllActiveByUserIdAsync(applicationUserId, ct);
        }

        public async Task<IList<ProspectListPhase>> GetAllActiveProspectListPhasesAsync(CancellationToken ct = default)
        {
            return await _prospectListPhaseRepository.GetAllActiveAsync(ct);
        }

        public async Task<IList<CampaignProspect>> GetAllCampaignProspectsByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.GetAllByCampaignIdAsync(campaignId, ct);
        }

        public async Task<IList<MonitorForNewConnectionsPhase>> GetAllMonitorForNewConnectionsPhasesByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _monitorForNewConnectionsPhaseRepository.GetAllByUserIdAsync(userId, ct);
        }

        public async Task<IList<SentConnectionsSearchUrlStatus>> GetAllSentConnectionsStatusesAsync(string campaignId, CancellationToken ct = default)
        {
            return await _sendConnectionsPhaseRepository.GetAllSentConnectionsStatusesAsync(campaignId, ct);
        }

        public async Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepository.GetByIdAsync(campaignId, ct);
        }

        public async Task<CampaignProspectList> GetCampaignProspectListByListIdAsync(string campaignProspectListId, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.GetListByListIdAsync(campaignProspectListId, ct);
        }

        public async Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepository.GetCampaignWarmUpByIdAsync(campaignId, ct);
        }

        public async Task<MonitorForNewConnectionsPhase> GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default)
        {
            return await _monitorForNewConnectionsPhaseRepository.GetBySocialAccountIdAsync(socialAccountId, ct);
        }

        public async Task<PrimaryProspect> GetPrimaryProspectByIdAsync(string primaryProspectId, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.GetByIdAsync(primaryProspectId, ct);
        }

        public async Task<PrimaryProspectList> GetPrimaryProspectListByIdAsync(string primaryProspectListId, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.GetListByIdAsync(primaryProspectListId, ct);    
        }

        public async Task<PrimaryProspectList> GetPrimaryProspectListByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.GetListByNameAndUserIdAsync(prospectListName, userId, ct);
        }

        public async Task<ProspectListPhase> GetProspectListPhaseByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _prospectListPhaseRepository.GetByCampaignIdAsync(campaignId, ct);
        }

        public async Task<ProspectListPhase> GetProspectListPhaseByIdAsync(string prospectListPhaseId, CancellationToken ct = default)
        {
            return await _prospectListPhaseRepository.GetByIdAsync(prospectListPhaseId, ct);
        }

        public async Task<IList<SendConnectionsStage>> GetStagesByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _sendConnectionsPhaseRepository.GetStagesByCampaignIdAsync(campaignId, ct);
        }

        public async Task<IList<CampaignProspect>> UpdateAllCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.UpdateAllAsync(campaignProspects, ct);
        }

        public async Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign, CancellationToken ct = default)
        {
            return await _campaignRepository.UpdateAsync(updatedCampaign, ct);
        }

        public async Task<ProspectListPhase> UpdateProspectListPhaseAsync(ProspectListPhase prospectListPhase, CancellationToken ct = default)
        {
            return await _prospectListPhaseRepository.UpdateAsync(prospectListPhase, ct);
        }

        public async Task<SentConnectionsSearchUrlStatus> UpdateSentConnectionsStatusAsync(SentConnectionsSearchUrlStatus updatedSearchUrlStatus, CancellationToken ct = default)
        {
            return await _sendConnectionsPhaseRepository.UpdateSentConnectionsStatusAsync(updatedSearchUrlStatus, ct);
        }

        public async Task<IList<FollowUpMessage>> GetFollowUpMessagesByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _followUpMessageRepository.GetAllByCampaignIdAsync(campaignId, ct);
        }

        public async Task<CampaignProspectFollowUpMessage> CreateFollowUpMessageAsync(CampaignProspectFollowUpMessage message, CancellationToken ct = default)
        {
            return await _followUpMessageRepository.CreateAsync(message, ct);
        }

        public async Task<CampaignProspectFollowUpMessage> GetCampaignProspectFollowUpMessageByIdAsync(string campaignProspectFollowUpMessageId, CancellationToken ct = default)
        {
            return await _followUpMessageRepository.GetCampaignProspectFollowUpMessageByIdAsync(campaignProspectFollowUpMessageId, ct);
        }

        public async Task<FollowUpMessagePhase> GetFollowUpMessagePhaseByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _followUpMessagePhaseRepository.GetByCampaignIdAsync(campaignId, ct);
        }
    }
}
