using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
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
        public CampaignProvider(ILogger<CampaignProvider> logger, IMemoryCache memoryCache, ICampaignService campaignService, ICampaignManager campaignManager, ICampaignRepository campaignRepository, ICloudPlatformRepository cloudPlatformRepository)
        {
            _campaignService = campaignService;
            _logger = logger;
            _memoryCache = memoryCache;
            _campaignManager = campaignManager;
            _campaignRepository = campaignRepository;
            _cloudPlatformRepository = cloudPlatformRepository;
        }

        private readonly ILogger<CampaignProvider> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ICampaignManager _campaignManager;
        private readonly ICampaignRepository _campaignRepository;
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

        public async Task<ProspectListBody> CreateProspectListBodyAsync(string prospectListPhaseId, string userId, CancellationToken ct = default)
        {
            ProspectListPhase prospectListPhase = await _campaignRepository.GetProspectListPhaseByPhaseIdAsync(prospectListPhaseId, ct);
            string primaryProspectListId = prospectListPhase.Campaign.CampaignProspectList.PrimaryProspectListId;
            string campaignId = prospectListPhase.CampaignId;

            string chromeProfileName = await _campaignRepository.GetChromeProfileNameByCampaignPhaseTypeAsync(PhaseType.ProspectList, ct);            
            if(chromeProfileName == null)
            {
                // create the new chrome profile name and save it
                ChromeProfileName profileName = new()
                {
                    CampaignPhaseType = PhaseType.ProspectList,
                    Profile = Guid.NewGuid().ToString()
                };
                await _campaignRepository.CreateChromeProfileNameAsync(profileName, ct);
                chromeProfileName = profileName.Profile;
            }

            if (_memoryCache.TryGetValue(CacheKeys.CloudPlatformConfigurationOptions, out CloudPlatformConfiguration config) == false)
            {
                config = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            }

            ProspectListBody prospectListBody = new()
            {
                SearchUrls = prospectListPhase.SearchUrls,
                HalId = prospectListPhase.Campaign.HalId,
                ChromeProfile = chromeProfileName,
                PrimaryProspectListId = primaryProspectListId,
                UserId = userId,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            return prospectListBody;
        }

        public async Task<MonitorForNewAcceptedConnectionsBody> CreateMonitorForNewAcceptedConnectionsBodyAsync(string userId, string halId, CancellationToken ct = default)
        {
            IList<MonitorForNewConnectionsPhase> monitorForNewConnectionsPhases = await _campaignRepository.GetAllMonitorForNewConnectionsPhasesByUserId(userId, ct);
            // we only care about the first phase
            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = monitorForNewConnectionsPhases.FirstOrDefault();

            string chromeProfileName = await _campaignRepository.GetChromeProfileNameByCampaignPhaseTypeAsync(PhaseType.MonitorNewConnections, ct);
            if (chromeProfileName == null)
            {
                // create the new chrome profile name and save it
                ChromeProfileName profileName = new()
                {
                    CampaignPhaseType = PhaseType.SendConnectionRequests,
                    Profile = Guid.NewGuid().ToString()
                };
                await _campaignRepository.CreateChromeProfileNameAsync(profileName, ct);
                chromeProfileName = profileName.Profile;
            }

            if (_memoryCache.TryGetValue(CacheKeys.CloudPlatformConfigurationOptions, out CloudPlatformConfiguration config) == false)
            {
                config = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            }

            MonitorForNewAcceptedConnectionsBody monitorForNewAcceptedConnectionsBody = new()
            {
                ChromeProfileName = chromeProfileName,                
                HalId = halId,
                UserId = userId,
                PageUrl = monitorForNewConnectionsPhase.PageUrl,
                StartWorkTime = DateTimeOffset.Parse("8:00 AM").ToUnixTimeSeconds(),
                EndWorkTime = DateTimeOffset.Parse("7:00 PM").ToUnixTimeSeconds(),
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            return monitorForNewAcceptedConnectionsBody;
        }

        public async Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            Campaign campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId, ct);
            int dailyConnectionsLimit = campaign.DailyInvites;
            if(campaign.IsWarmUpEnabled == true)
            {
                CampaignWarmUp campaignWarmUp = await _campaignRepository.GetCampaignWarmUpByIdAsync(campaignId, ct);
                dailyConnectionsLimit = campaignWarmUp.DailyLimit;
            }                  

            string chromeProfileName = await _campaignRepository.GetChromeProfileNameByCampaignPhaseTypeAsync(PhaseType.SendConnectionRequests, ct);
            if (chromeProfileName == null)
            {
                // create the new chrome profile name and save it
                ChromeProfileName profileName = new()
                {
                    CampaignPhaseType = PhaseType.SendConnectionRequests,
                    Profile = Guid.NewGuid().ToString()
                };
                await _campaignRepository.CreateChromeProfileNameAsync(profileName, ct);
                chromeProfileName = profileName.Profile;
            }

            if (_memoryCache.TryGetValue(CacheKeys.CloudPlatformConfigurationOptions, out CloudPlatformConfiguration config) == false)
            {
                config = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            }

            SendConnectionsBody sendConnectionsBody = new()
            {
                ChromeProfileName = chromeProfileName,
                DailyLimit = dailyConnectionsLimit,
                HalId = campaign.HalId,
                UserId = userId,
                StartDateTimestamp = campaign.StartTimestamp,       
                CampaignId = campaignId,                                
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            return sendConnectionsBody;
        }

        public async Task<IList<SendConnectionsStageBody>> GetSendConnectionsStagesAsync(string campaignId, int dailyConnectionsLimit, CancellationToken ct = default)
        {
            IList<SendConnectionsStageBody> sendConnectionsStagesBody = new List<SendConnectionsStageBody>();
            IList<SendConnectionsStage> sendConnectionsStages = await _campaignRepository.GetSendConnectionStagesByIdAsync(campaignId, ct);

            decimal totalInvites = dailyConnectionsLimit;
            int divider = sendConnectionsStages.Count;
            List<int> stagesConnectionsLimit = new();
            while (divider > 0)
            {
                decimal stageLimits = Math.Round(totalInvites / sendConnectionsStages.Count, 0);
                stagesConnectionsLimit.Add(Convert.ToInt32(stageLimits));
                divider--;
            }

            for (int i = 0; i < sendConnectionsStages.Count; i++)
            {
                SendConnectionsStageBody sendConnectionsStageBody = new()
                {
                    ConnectionsLimit = stagesConnectionsLimit[i],
                    StartTime = sendConnectionsStages[i].StartTime,
                    Order = sendConnectionsStages[i].Order
                };

                sendConnectionsStagesBody.Add(sendConnectionsStageBody);
            }

            return sendConnectionsStagesBody;
        }

        public CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId)
        {
            CampaignProspectList campaignProspectList = _campaignService.GenerateCampaignProspectList(primaryProspectList, userId);

            return campaignProspectList;
        }

        public async Task<HalsProspectListPhasesPayload> GetActiveProspectListPhasesAsync(CancellationToken ct = default)
        {
            List<ProspectListPhase> prospectListPhases = await _campaignRepository.GetAllActivePropspectListPhasesAsync(ct);

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

        public async Task<List<string>> HalIdsWithActiveCampaignsAsync(CancellationToken ct = default)
        {
            List<Campaign> activeCampaigns = await _campaignRepository.GetAllActiveAsync(ct);

            List<string> halIds = activeCampaigns.Select(c => c.HalId).ToList();

            return halIds;
        }

        public async Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken ct = default)
        {
            return await _campaignRepository.GetAllActiveAsync(ct);
        }
        
    }
}
