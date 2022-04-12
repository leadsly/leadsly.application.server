using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CampaignProvider : ICampaignProvider
    {
        public CampaignProvider(
            ILogger<CampaignProvider> logger, 
            IMemoryCache memoryCache, 
            ICampaignService campaignService, 
            ICampaignManager campaignManager, 
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ICloudPlatformRepository cloudPlatformRepository       
            )
        {
            _campaignService = campaignService;
            _logger = logger;
            _memoryCache = memoryCache;
            _campaignManager = campaignManager;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _cloudPlatformRepository = cloudPlatformRepository;            
        }

        private readonly ILogger<CampaignProvider> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ICampaignManager _campaignManager;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;        
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ICampaignService _campaignService;        

        public void ProcessNewCampaign(Campaign campaign)
        {
            // ensure ScanForProspectReplies, ConnectionWithdraw and MonitorForNewProspects phases are running on hal
            // always trigger them here


            // if prospect list phase does not exists, this means were running new prospect list
            if(campaign.ProspectListPhase == null)
            {                
                _campaignManager.TriggerSendConnectionsPhase(campaign.CampaignId, campaign.ApplicationUserId);
            }
            else
            {
                _campaignManager.TriggerProspectListPhase(campaign.ProspectListPhase.ProspectListPhaseId, campaign.ApplicationUserId);
            }
        }

        public CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId)
        {
            CampaignProspectList campaignProspectList = _campaignService.GenerateCampaignProspectList(primaryProspectList, userId);

            return campaignProspectList;
        }

        public async Task<HalsProspectListPhasesPayload> GetActiveProspectListPhasesAsync(CancellationToken ct = default)
        {
            IList<ProspectListPhase> prospectListPhases = await _campaignRepositoryFacade.GetAllActiveProspectListPhasesAsync(ct);

            IEnumerable<string> halIds = prospectListPhases.Select(phase => phase.Campaign.HalId).Distinct();

            HalsProspectListPhasesPayload halsPhases = new();
            foreach (string halId in halIds)
            {
                List<ProspectListBody> content = prospectListPhases.Where(p => p.Campaign.HalId == halId).Select(p =>
                {
                    return new ProspectListBody
                    {
                        SearchUrls = p.SearchUrls
                    };
                }).ToList();

                halsPhases.ProspectListPayload.Add(halId, content);
            }

            return halsPhases;
        }

        public async Task<List<string>> GetHalIdsWithActiveCampaignsAsync(CancellationToken ct = default)
        {
            IList<Campaign> activeCampaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsAsync(ct);

            List<string> halIds = activeCampaigns.Select(c => c.HalId).ToList();

            return halIds;
        }

        public async Task<int> CreateDailyWarmUpLimitConfigurationAsync(long startDateTimestamp, CancellationToken ct = default)
        {
            return await _campaignService.CreateDailyWarmUpLimitConfigurationAsync(startDateTimestamp, ct);
        }

        public void TriggerSendConnectionsPhase(string campaignId, string userId)
        {
            _campaignManager.TriggerSendConnectionsPhase(campaignId, userId);
        }
    }
}
