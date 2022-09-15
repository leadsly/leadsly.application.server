using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
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

        public async Task<PublishMessageBody> CreateMQMessageAsync(string halId, string startTime, int numberOfProspectsToCrawl, Models.Entities.Campaigns.Campaign campaign, Models.Entities.Campaigns.PrimaryProspectList primaryProspectList, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating {0} MQ message for HalId {1}.", nameof(NetworkingMessageBody), halId);
            string userId = campaign.ApplicationUserId;
            string campaignId = campaign.CampaignId;

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(campaign.HalId, ct);
            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halUnit.HalId, ct);
            EcsService gridEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == Purpose.Grid);
            EcsService proxyEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == Purpose.Proxy);
            string virtualAssistantId = virtualAssistant.VirtualAssistantId;
            if (gridEcsService == null || proxyEcsService == null)
            {
                throw new Exception($"Ecs services not found for virtual assistant {virtualAssistantId}.");
            }

            if (gridEcsService.CloudMapDiscoveryService == null || proxyEcsService.CloudMapDiscoveryService == null)
            {
                throw new Exception($"Cloud map discovery services not found for virtual assistant {virtualAssistantId}.");
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
                ProxyServiceDiscoveryName = proxyEcsService.CloudMapDiscoveryService.Name,
                ProxyNamespaceName = config.ServiceDiscoveryConfig.Proxy.Name,
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
