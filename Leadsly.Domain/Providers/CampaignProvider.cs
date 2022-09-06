using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CampaignProvider : ICampaignProvider
    {
        public CampaignProvider(
            ILogger<CampaignProvider> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHangfireService hangfireService,
            IMemoryCache memoryCache,
            IFollowUpMessageJobsRepository followUpMessageJobRepository
            )
        {
            _hangfireService = hangfireService;
            _followUpMessageJobRepository = followUpMessageJobRepository;
            _memoryCache = memoryCache;
            _logger = logger;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IHangfireService _hangfireService;
        private readonly IFollowUpMessageJobsRepository _followUpMessageJobRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CampaignProvider> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;

        /// <summary>
        /// Grabs all ProspectListPhases that have not completed. Each campaign that creates new PropsectList will have
        /// a ProspectListPhase. ProspectListPhase is triggered first to gather all prospects in the given search urls.
        /// If campaign is created after Hal's work hours, the ProspectListPhase is not triggered until the next work day.
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns></returns>
        public async Task<IList<ProspectListPhase>> GetIncompleteProspectListPhasesAsync(string halId, CancellationToken ct = default)
        {
            IList<ProspectListPhase> prospectListPhases = await _campaignRepositoryFacade.GetAllActiveProspectListPhasesAsync(ct);
            IList<ProspectListPhase> incompleteProspectListPhases = prospectListPhases.Where(p => p.Completed == false).ToList();

            return incompleteProspectListPhases;
        }

        public async Task<IList<Campaign>> GetAllByUserIdAsync(string userId, CancellationToken ct = default)
        {
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllCampaignsByUserIdAsync(userId, ct);

            return campaigns;
        }

        public async Task<long> GetTotalConnectionsSentAsync(string campaignId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetCampaignProspectsAsync(campaignId, ct);
            IList<CampaignProspect> setnConnectionsProspects = campaignProspects?.Where(p => p.ConnectionSent == true).ToList();

            if (setnConnectionsProspects == null || setnConnectionsProspects.Count == 0)
            {
                _logger.LogInformation("Campaign {campaignId} has no sent connections", campaignId);
                return 0;
            }

            return setnConnectionsProspects.Count;
        }

        public async Task<long> GetConnectionsAcceptedAsync(string campaignId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetCampaignProspectsAsync(campaignId, ct);
            IList<CampaignProspect> connectionsAccepted = campaignProspects?.Where(p => p.Accepted == true).ToList();

            if (connectionsAccepted == null || connectionsAccepted.Count == 0)
            {
                _logger.LogInformation("Campaign {campaignId} has no accepted connections", campaignId);
                return 0;
            }

            return connectionsAccepted.Count;
        }

        public async Task<long> GetRepliesAsync(string campaignId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetCampaignProspectsAsync(campaignId, ct);
            IList<CampaignProspect> connectionReplied = campaignProspects?.Where(p => p.Replied == true).ToList();

            if (connectionReplied == null || connectionReplied.Count == 0)
            {
                _logger.LogInformation("Campaign {campaignId} has no prospects that replied to any of the messages yet.", campaignId);
                return 0;
            }

            return connectionReplied.Count;
        }

        public async Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);
        }

        public async Task<Campaign> UpdateCampaignAsync(Campaign campaign, CancellationToken ct = default)
        {
            campaign = await _campaignRepositoryFacade.UpdateCampaignAsync(campaign, ct);
            if (campaign == null)
            {
                _logger.LogError("Failed to update campaign {campaignId}", campaign.CampaignId);
            }
            return campaign;
        }

        public async Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepositoryFacade.DeleteCampaignAsync(campaignId, ct);
        }

        private async Task<IList<CampaignProspect>> GetCampaignProspectsAsync(string campaignId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue($"CampaignProspects-{campaignId}", out IList<CampaignProspect> campaignProspects) == false)
            {
                campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(campaignId);
                _memoryCache.Set($"CampaignProspects-{campaignId}", campaignProspects, TimeSpan.FromMinutes(3));
            }

            return campaignProspects;
        }
    }
}
