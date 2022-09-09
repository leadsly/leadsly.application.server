using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class NetworkingMessagesFactory : INetworkingMessagesFactory
    {
        public NetworkingMessagesFactory(
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider,
            IVirtualAssistantRepository virtualAssistantRepository,
            ILogger<NetworkingMessagesFactory> logger)
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _rabbitMQProvider = rabbitMQProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _halRepository = halRepository;
            _logger = logger;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IHalRepository _halRepository;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly ILogger<NetworkingMessagesFactory> _logger;

        public async Task<IList<NetworkingMessageBody>> CreateNetworkingMessagesAsync(string halId, CancellationToken ct = default)
        {
            List<NetworkingMessageBody> messages = new List<NetworkingMessageBody>();
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);
            foreach (Campaign activeCampaign in campaigns)
            {
                // filter here based on searchurlprogress
                if (activeCampaign.SearchUrlsProgress.Any(s => s.Exhausted == false))
                {
                    IList<NetworkingMessageBody> msgs = await CreateNetworkingMessagesAsync(activeCampaign.CampaignId, activeCampaign.ApplicationUserId);
                    messages.AddRange(msgs);
                }
            }

            return messages;
        }

        public async Task<IList<NetworkingMessageBody>> CreateNetworkingMessagesAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            IList<NetworkingMessageBody> messages = new List<NetworkingMessageBody>();
            IList<IDictionary<int, string>> propsectsToCrawlAndTimes = await GetNumberOfNetworkingPhasesAsync(campaignId);
            if (propsectsToCrawlAndTimes.Count == 0)
            {
                _logger.LogDebug("None of the prospects to crawl and their times to crawl and connect with were determined from [GetNumberOfNetworkingPhasesAsync] method.");
            }

            foreach (var propsectsToCrawlAndTime in propsectsToCrawlAndTimes)
            {
                foreach (var item in propsectsToCrawlAndTime)
                {
                    int numberOfProspectsToCrawl = item.Key;
                    string startTime = item.Value;
                    NetworkingMessageBody message = await CreateNetworkingMessagesAsync(campaignId, userId, numberOfProspectsToCrawl, startTime, ct);
                    messages.Add(message);
                }
            }

            if (messages.Count == 0)
            {
                _logger.LogInformation("No networking messages were generated for CampaignId {campaignId} with UserId {userId}", campaignId, userId);
            }

            return messages;
        }

        private async Task<IList<IDictionary<int, string>>> GetNumberOfNetworkingPhasesAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogDebug("Determining number of networking phases for campaign {campaignId}. This will return number of prospects we should connect with and what time.", campaignId);
            IList<IDictionary<int, string>> propsectsToCrawlAndTime = new List<IDictionary<int, string>>();
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);
            if (campaign.IsWarmUpEnabled == true)
            {
                throw new NotImplementedException();
            }
            else
            {
                IList<SendConnectionsStage> sendConnectionsStages = await _campaignRepositoryFacade.GetStagesByCampaignIdAsync(campaignId, ct);
                _logger.LogDebug($"Number of SendConnectionsStages is: {sendConnectionsStages.Count}");

                int invitesPerStage = Math.DivRem(campaign.DailyInvites, sendConnectionsStages.Count, out int remainderInvites);
                _logger.LogDebug("Number of invites that should happen per stage is {invitesPerStage}", invitesPerStage);
                _logger.LogTrace("The remainder of invites is {remainderInvites}", remainderInvites);

                List<int> stagesConnectionsLimit = sendConnectionsStages.OrderBy(s => s.Order).Select(_ => invitesPerStage).ToList();
                if (remainderInvites != 0)
                {
                    int last = stagesConnectionsLimit.Last();
                    stagesConnectionsLimit.RemoveAt(stagesConnectionsLimit.Count - 1);
                    last += remainderInvites;
                    stagesConnectionsLimit.Add(last);
                }

                for (int i = 0; i < stagesConnectionsLimit.Count; i++)
                {
                    int numberOfPhases = Math.DivRem(stagesConnectionsLimit[i], 10, out int remainder);
                    // numberOfPhases becomes the number of Networking phases we need to trigger for this stage
                    for (int j = 0; j < numberOfPhases; j++)
                    {
                        propsectsToCrawlAndTime.Add(
                                new Dictionary<int, string>()
                                {
                                    { 10, sendConnectionsStages[i].StartTime }
                                }
                            );
                    }
                    if (remainder != 0)
                    {
                        propsectsToCrawlAndTime.Add(
                                new Dictionary<int, string>()
                                {
                                    { remainder, sendConnectionsStages[i].StartTime }
                                }
                            );
                    }
                }
            }

            return propsectsToCrawlAndTime;
        }

        private async Task<NetworkingMessageBody> CreateNetworkingMessagesAsync(string campaignId, string userId, int numberOfProspectsToCrawl, string startTime, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);
            PrimaryProspectList primaryProspectList = await _campaignRepositoryFacade.GetPrimaryProspectListByIdAsync(campaign.CampaignProspectList.PrimaryProspectListId, ct);
            string halId = campaign.HalId;
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(campaign.HalId, ct);
            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halUnit.HalId, ct);
            EcsService gridEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == Purpose.Grid);
            string virtualAssistantId = virtualAssistant.VirtualAssistantId;
            if (gridEcsService == null)
            {
                throw new Exception($"Grid ecs service not found for virtual assistant {virtualAssistantId}.");
            }

            if (gridEcsService.CloudMapDiscoveryService == null)
            {
                throw new Exception($"Cloud map discovery service not found for virtual assistant {virtualAssistantId}.");
            }

            int dailyConnectionsLimit = campaign.DailyInvites;
            _logger.LogDebug("Daily connection request limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            if (campaign.IsWarmUpEnabled == true)
            {
                _logger.LogDebug("Warm up is enabled. Retrieving warm up object from the database");
                CampaignWarmUp campaignWarmUp = await _campaignRepositoryFacade.GetCampaignWarmUpByIdAsync(campaignId, ct);
                dailyConnectionsLimit = campaignWarmUp.DailyLimit;
                _logger.LogDebug("Daily connection has been updated because warm up is enabled. Current daily warm up limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            }

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.Networking, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.Networking, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.SendConnectionRequests is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            NetworkingMessageBody sendConnectionsBody = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                ChromeProfileName = chromeProfileName,
                HalId = halUnit.HalId,
                UserId = userId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                CampaignProspectListId = campaign.CampaignProspectList.CampaignProspectListId,
                PrimaryProspectListId = primaryProspectList.PrimaryProspectListId,
                StartTime = startTime,
                ProspectsToCrawl = numberOfProspectsToCrawl,
                EndOfWorkday = halUnit.EndHour,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                SocialAccountId = virtualAssistant.SocialAccount.SocialAccountId
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("NetworkingMessageBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is {halId}", gridNamespaceName, halId);
            _logger.LogTrace("NetworkingMessageBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("NetworkingMessageBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("NetworkingMessageBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is {halId}", appServerServiceDiscoveryname, halId);

            return sendConnectionsBody;
        }

        public async Task<PublishMessageBody> CreateMQMessageAsync(string halId, string startTime, int numberOfProspectsToCrawl, Models.Entities.Campaigns.Campaign campaign, Models.Entities.Campaigns.PrimaryProspectList primaryProspectList, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating {0} MQ message for HalId {1}.", nameof(NetworkingMessageBody), halId);
            string userId = campaign.ApplicationUserId;
            string campaignId = campaign.CampaignId;

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(campaign.HalId, ct);
            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halUnit.HalId, ct);
            EcsService gridEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == Purpose.Grid);
            string virtualAssistantId = virtualAssistant.VirtualAssistantId;
            if (gridEcsService == null)
            {
                throw new Exception($"Grid ecs service not found for virtual assistant {virtualAssistantId}.");
            }

            if (gridEcsService.CloudMapDiscoveryService == null)
            {
                throw new Exception($"Cloud map discovery service not found for virtual assistant {virtualAssistantId}.");
            }

            int dailyConnectionsLimit = campaign.DailyInvites;
            _logger.LogDebug("Daily connection request limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            if (campaign.IsWarmUpEnabled == true)
            {
                throw new NotImplementedException();
            }

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.Networking, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                ChromeProfile profileName = new()
                {
                    CampaignPhaseType = PhaseType.Networking,
                    Name = Guid.NewGuid().ToString()
                };
                await _halRepository.CreateChromeProfileAsync(profileName, ct);
                chromeProfileName = profileName.Name;
            }
            _logger.LogDebug($"The chrome profile used for {nameof(PhaseType.SendConnectionRequests)} is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            NetworkingMessageBody mqMessage = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                ChromeProfileName = chromeProfileName,
                HalId = halId,
                UserId = userId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                CampaignProspectListId = campaign.CampaignProspectList.CampaignProspectListId,
                PrimaryProspectListId = primaryProspectList.PrimaryProspectListId,
                StartTime = startTime,
                ProspectsToCrawl = numberOfProspectsToCrawl,
                EndOfWorkday = halUnit.EndHour,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                SocialAccountId = virtualAssistant.SocialAccount.SocialAccountId
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("NetworkingMessageBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is {halId}", gridNamespaceName, halId);
            _logger.LogTrace("NetworkingMessageBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("NetworkingMessageBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("NetworkingMessageBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is {halId}", appServerServiceDiscoveryname, halId);

            return mqMessage;
            ;
        }
    }
}
