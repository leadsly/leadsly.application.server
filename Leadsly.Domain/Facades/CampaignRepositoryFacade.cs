using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades
{
    public class CampaignRepositoryFacade : ICampaignRepositoryFacade
    {
        public CampaignRepositoryFacade(
            ICampaignProspectRepository campaignProspectRepository,
            ICampaignRepository campaignRepository,
            IFollowUpMessagePhaseRepository followUpMessagePhaseRepository,
            IMonitorForNewConnectionsPhaseRepository monitorForNewConnectionsPhaseRepository,
            IPrimaryProspectRepository primaryProspectRepository,
            IProspectListPhaseRepository prospectListPhaseRepository,
            IScanProspectsForRepliesPhaseRepository scanProspectsForRepliesPhaseRepository,
            IFollowUpMessageRepository followUpMessageRepository,
            ISendConnectionsPhaseRepository sendConnectionsPhaseRepository
            )
        {
            _campaignProspectRepository = campaignProspectRepository;
            _sendConnectionsPhaseRepository = sendConnectionsPhaseRepository;
            _campaignRepository = campaignRepository;
            _followUpMessageRepository = followUpMessageRepository;
            _followUpMessagePhaseRepository = followUpMessagePhaseRepository;
            _monitorForNewConnectionsPhaseRepository = monitorForNewConnectionsPhaseRepository;
            _primaryProspectRepository = primaryProspectRepository;
            _prospectListPhaseRepository = prospectListPhaseRepository;
            _scanProspectsForRepliesPhaseRepository = scanProspectsForRepliesPhaseRepository;
        }

        private readonly ISendConnectionsPhaseRepository _sendConnectionsPhaseRepository;
        private readonly ICampaignProspectRepository _campaignProspectRepository;
        private readonly IFollowUpMessageRepository _followUpMessageRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IFollowUpMessagePhaseRepository _followUpMessagePhaseRepository;
        private readonly IMonitorForNewConnectionsPhaseRepository _monitorForNewConnectionsPhaseRepository;
        private readonly IPrimaryProspectRepository _primaryProspectRepository;
        private readonly IProspectListPhaseRepository _prospectListPhaseRepository;
        private readonly IScanProspectsForRepliesPhaseRepository _scanProspectsForRepliesPhaseRepository;

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

        public async Task<PrimaryProspectList> CreatePrimaryProspectListAsync(PrimaryProspectList primaryProspectList, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.CreateListAsync(primaryProspectList, ct);
        }


        public async Task<IList<Campaign>> GetAllActiveCampaignsAsync(CancellationToken ct = default)
        {
            return await _campaignRepository.GetAllActiveAsync(ct);
        }


        public async Task<IList<Campaign>> GetAllCampaignsByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _campaignRepository.GetAllByUserIdAsync(userId, ct);
        }

        public async Task<IList<Campaign>> GetAllActiveCampaignsByHalIdAsync(string halId, CancellationToken ct = default)
        {
            return await _campaignRepository.GetAllActiveByHalIdAsync(halId, ct);
        }

        public async Task<IList<ProspectListPhase>> GetAllActiveProspectListPhasesAsync(CancellationToken ct = default)
        {
            return await _prospectListPhaseRepository.GetAllActiveAsync(ct);
        }

        public async Task<IList<CampaignProspect>> GetAllCampaignProspectsByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.GetAllByCampaignIdAsync(campaignId, ct);
        }

        public async Task<CampaignProspect> GetCampaignProspectByProfileUrlAsync(string profileUrl, string halId, string userId, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.GetByProfileUrlAsync(profileUrl, halId, userId, ct);
        }

        public async Task<IList<CampaignProspect>> GetAllActiveCampaignProspectsByHalIdAsync(string halId, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.GetAllActiveByHalIdAsync(halId, ct);
        }

        public async Task<IList<CampaignProspect>> GetAllFollowUpMessageEligbleProspectsByCampaignIdAsync(string campaignId, CancellationToken ct)
        {
            return await _campaignProspectRepository.GetAllFollowUpMessageEligbleByCampaignIdAsync(campaignId, ct);
        }

        public async Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepository.GetByIdAsync(campaignId, ct);
        }

        public async Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepository.GetCampaignWarmUpByIdAsync(campaignId, ct);
        }

        public async Task<MonitorForNewConnectionsPhase> GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default)
        {
            return await _monitorForNewConnectionsPhaseRepository.GetBySocialAccountIdAsync(socialAccountId, ct);
        }

        public async Task<PrimaryProspectList> GetPrimaryProspectListByIdAsync(string primaryProspectListId, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.GetListByIdAsync(primaryProspectListId, ct);
        }

        public async Task<bool> AnyActiveCampaignsByHalIdAsync(string halId, CancellationToken ct = default)
        {
            return await _campaignRepository.AnyActiveByHalIdAsync(halId, ct);
        }

        public async Task<PrimaryProspectList> GetPrimaryProspectListByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default)
        {
            return await _primaryProspectRepository.GetListByNameAndUserIdAsync(prospectListName, userId, ct);
        }

        public async Task<ProspectListPhase> GetProspectListPhaseByIdAsync(string prospectListPhaseId, CancellationToken ct = default)
        {
            return await _prospectListPhaseRepository.GetByIdAsync(prospectListPhaseId, ct);
        }

        public async Task<IList<CampaignProspect>> UpdateAllCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.UpdateAllAsync(campaignProspects, ct);
        }

        public async Task<CampaignProspect> UpdateCampaignProspectAsync(CampaignProspect updatedCampaignProspect, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.UpdateAsync(updatedCampaignProspect, ct);
        }

        public async Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign, CancellationToken ct = default)
        {
            return await _campaignRepository.UpdateAsync(updatedCampaign, ct);
        }

        public async Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepository.DeleteAsync(campaignId, ct);
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

        public async Task<ScanProspectsForRepliesPhase> GetScanProspectsForRepliesPhaseByIdAsync(string scanProspectsForRepliesPhaseId, CancellationToken ct = default)
        {
            return await _scanProspectsForRepliesPhaseRepository.GetByIdAsync(scanProspectsForRepliesPhaseId, ct);
        }

        public async Task<CampaignProspect> GetCampaignProspectByIdAsync(string campaignProspectId, CancellationToken ct = default)
        {
            return await _campaignProspectRepository.GetByIdAsync(campaignProspectId, ct);
        }

        public async Task<bool> AnyIncompleteProspectListPhasesByHalIdAsync(string halId, CancellationToken ct = default)
        {
            return await _prospectListPhaseRepository.AnyIncompleteByHalIdAsync(halId, ct);
        }

        public async Task<bool> DeleteMonitorForNewConnectionsPhaseAsync(string monitorForNewConnectionsPhaseId, CancellationToken ct = default)
        {
            return await _monitorForNewConnectionsPhaseRepository.DeleteAsync(monitorForNewConnectionsPhaseId, ct);
        }

        public async Task<bool> DeleteScanProspectsForRepliesPhaseAsync(string scanProspectsForRepliesPhaseId, CancellationToken ct = default)
        {
            return await _scanProspectsForRepliesPhaseRepository.DeleteAsync(scanProspectsForRepliesPhaseId, ct);
        }

        public async Task<IList<SendConnectionsStage>> GetStagesByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _sendConnectionsPhaseRepository.GetStagesByCampaignIdAsync(campaignId, ct);
        }
    }
}
