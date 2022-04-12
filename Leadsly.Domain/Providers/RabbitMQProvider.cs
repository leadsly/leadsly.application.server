using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
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
    public class RabbitMQProvider : IRabbitMQProvider
    {
        public RabbitMQProvider(
            ILogger<RabbitMQProvider> logger, 
            IMemoryCache memoryCache, 
            ICampaignRepository campaignRepository, 
            IHalRepository halRepository,
            IMonitorForNewConnectionsPhaseRepository monitorForNewConnectionsPhaseRepository,
            ISendConnectionsPhaseRepository sendConnectionsPhaseRepository,
            ICloudPlatformRepository cloudPlatformRepository,
            IProspectListPhaseRepository prospectListPhaseRepository)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _sendConnectionsPhaseRepository = sendConnectionsPhaseRepository;
            _halRepository = halRepository;
            _campaignRepository = campaignRepository;
            _cloudPlatformRepository = cloudPlatformRepository;
            _monitorForNewConnectionsPhaseRepository = monitorForNewConnectionsPhaseRepository;
            _prospectListPhaseRepository = prospectListPhaseRepository;
        }

        private readonly ILogger<RabbitMQProvider> _logger;
        private readonly IHalRepository _halRepository;
        private readonly ISendConnectionsPhaseRepository _sendConnectionsPhaseRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IMonitorForNewConnectionsPhaseRepository _monitorForNewConnectionsPhaseRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly IProspectListPhaseRepository _prospectListPhaseRepository;
        private readonly ICampaignRepository _campaignRepository;

        public async Task<IList<SendConnectionsStageBody>> GetSendConnectionsStagesAsync(string campaignId, int dailyConnectionsLimit, CancellationToken ct = default)
        {
            _logger.LogInformation("Getting the number of connections each stage should sent to stay within the daily limit");
            IList<SendConnectionsStageBody> sendConnectionsStagesBody = new List<SendConnectionsStageBody>();
            IList<SendConnectionsStage> sendConnectionsStages = await _sendConnectionsPhaseRepository.GetStagesByCampaignIdAsync(campaignId, ct);

            decimal totalInvites = dailyConnectionsLimit;
            int divider = sendConnectionsStages.Count;
            List<int> stagesConnectionsLimit = new();
            while (divider > 0)
            {
                decimal stageLimits = Math.Round(totalInvites / sendConnectionsStages.Count, 0);
                int currentStageLimits = Convert.ToInt32(stageLimits);                
                stagesConnectionsLimit.Add(currentStageLimits);
                divider--;
            }

            int numOfStages = sendConnectionsStages.Count;
            _logger.LogDebug("Number of send connection stages for this campaign is {numOfStages}", numOfStages);
            for (int i = 0; i < numOfStages; i++)
            {
                int order = sendConnectionsStages[i].Order;
                int stageConnectionsLimit = stagesConnectionsLimit[i];

                SendConnectionsStageBody sendConnectionsStageBody = new()
                {
                    ConnectionsLimit = stageConnectionsLimit,
                    StartTime = sendConnectionsStages[i].StartTime,
                    Order = order
                };

                _logger.LogDebug("This send connections stage has the order of {order}. The limit is {stageConnectionsLimit}", order, stageConnectionsLimit);

                sendConnectionsStagesBody.Add(sendConnectionsStageBody);
            }

            return sendConnectionsStagesBody;
        }
        public async Task<ProspectListBody> CreateProspectListBodyAsync(string prospectListPhaseId, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating prospect list body message for rabbit mq message broker.");
            ProspectListPhase prospectListPhase = await _prospectListPhaseRepository.GetByIdAsync(prospectListPhaseId, ct);
            string primaryProspectListId = prospectListPhase.Campaign.CampaignProspectList.PrimaryProspectListId;
            _logger.LogDebug("Creating prospect list body with primary prospect list id {primaryProspectListId}", primaryProspectListId);
            string campaignId = prospectListPhase.CampaignId;
            _logger.LogDebug("Creating prospect list body with campaign id {campaignId}", campaignId);

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.ProspectList, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await CreateNewChromeProfileAsync(PhaseType.ProspectList, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.ProspectList is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = GetCloudPlatformConfiguration();

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

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("ProspectListBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("ProspectListBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return prospectListBody;
        }
        public async Task<MonitorForNewAcceptedConnectionsBody> CreateMonitorForNewAcceptedConnectionsBodyAsync(string halId, string userId, string socialAccountId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating monitor for new connections body message for rabbit mq message broker.");
            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = await _monitorForNewConnectionsPhaseRepository.GetBySocialAccountIdAsync(socialAccountId, ct);

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.MonitorNewConnections, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await CreateNewChromeProfileAsync(PhaseType.MonitorNewConnections, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.MonitorNewConnections is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = GetCloudPlatformConfiguration();

            MonitorForNewAcceptedConnectionsBody monitorForNewAcceptedConnectionsBody = new()
            {
                ChromeProfileName = chromeProfileName,
                HalId = halId,
                UserId = userId,
                TimeZoneId = "Eastern Standard Time",
                PageUrl = monitorForNewConnectionsPhase.PageUrl,
                StartWorkTime = new DateTimeOffset(new DateTimeWithZone(DateTime.Parse("7:00 AM"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")).LocalTime).ToUnixTimeSeconds(), // DateTimeOffset.Parse("8:00 AM").ToUnixTimeSeconds(),
                EndWorkTime = new DateTimeOffset(new DateTimeWithZone(DateTime.Parse("7:00 PM"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")).LocalTime).ToUnixTimeSeconds(), // DateTimeOffset.Parse("7:00 PM").ToUnixTimeSeconds(),
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return monitorForNewAcceptedConnectionsBody;
        }
        public async Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            Campaign campaign = await _campaignRepository.GetByIdAsync(campaignId, ct);
            int dailyConnectionsLimit = campaign.DailyInvites;
            _logger.LogDebug("Daily connection request limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            if (campaign.IsWarmUpEnabled == true)
            {
                _logger.LogDebug("Warm up is enabled. Retrieving warm up object from the database");
                CampaignWarmUp campaignWarmUp = await _campaignRepository.GetCampaignWarmUpByIdAsync(campaignId, ct);                
                dailyConnectionsLimit = campaignWarmUp.DailyLimit;
                _logger.LogDebug("Daily connection has been updated because warm up is enabled. Current daily warm up limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            }


            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.SendConnectionRequests, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await CreateNewChromeProfileAsync(PhaseType.SendConnectionRequests, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.SendConnectionRequests is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = GetCloudPlatformConfiguration();

            SendConnectionsBody sendConnectionsBody = new()
            {
                ChromeProfileName = chromeProfileName,
                DailyLimit = dailyConnectionsLimit,
                HalId = campaign.HalId,
                UserId = userId,
                TimeZoneId = "Eastern Standard Time",
                StartDateTimestamp = campaign.StartTimestamp,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("SendConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("SendConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return sendConnectionsBody;
        }

        private async Task<string> CreateNewChromeProfileAsync(PhaseType phaseType, CancellationToken ct = default)
        {
            string phaseName = Enum.GetName(phaseType);
            _logger.LogDebug("There is no chrome profile entry created for {phaseName}. Creating a new chrome profile name", phaseName);
            // create the new chrome profile name and save it
            ChromeProfile profileName = new()
            {
                CampaignPhaseType = PhaseType.ProspectList,
                Name = Guid.NewGuid().ToString()
            };
            await _halRepository.CreateChromeProfileAsync(profileName, ct);
            string chromeProfileName = profileName.Name;

            _logger.LogInformation("Using the following chrome profile name {chromeProfileName}", chromeProfileName);

            return chromeProfileName;
        }

        private CloudPlatformConfiguration GetCloudPlatformConfiguration()
        {
            if (_memoryCache.TryGetValue(CacheKeys.CloudPlatformConfigurationOptions, out CloudPlatformConfiguration config) == false)
            {
                _logger.LogDebug("Aws configuration options have not been loaded yet. Retrieving them from the database.");
                config = _cloudPlatformRepository.GetCloudPlatformConfiguration();
                _logger.LogDebug("Adding Aws configuration options to memory cache.");
                _memoryCache.Set(CacheKeys.CloudPlatformConfigurationOptions, config);
            }

            return config;
        }
    }
}
