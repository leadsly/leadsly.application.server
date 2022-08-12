using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class FollowUpMessagesFactory : IFollowUpMessagesFactory
    {
        public FollowUpMessagesFactory(
            ILogger<FollowUpMessagesFactory> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
            _halRepository = halRepository;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<FollowUpMessagesFactory> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;

        public async Task<FollowUpMessageBody> CreateMessageAsync(string campaignProspectFollowUpMessageId, string campaignId, CancellationToken ct = default)
        {
            return await CreateFollowUpMessageBodyAsync(campaignProspectFollowUpMessageId, campaignId, ct); ;
        }

        private async Task<FollowUpMessageBody> CreateFollowUpMessageBodyAsync(string campaignProspectFollowUpMessageId, string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating follow up message body for rabbit mq message broker.");

            FollowUpMessagePhase followUpMessagePhase = await _campaignRepositoryFacade.GetFollowUpMessagePhaseByCampaignIdAsync(campaignId, ct);
            CampaignProspectFollowUpMessage followUpMessage = await _campaignRepositoryFacade.GetCampaignProspectFollowUpMessageByIdAsync(campaignProspectFollowUpMessageId, ct);
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(followUpMessagePhase.Campaign.HalId);
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

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.FollwUpMessage, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.FollwUpMessage, ct);
            }

            _logger.LogDebug("The chrome profile used for PhaseType.FollowUpMessage is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            FollowUpMessageBody message = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                HalId = followUpMessagePhase.Campaign.HalId,
                Content = followUpMessage.Content,
                UserId = followUpMessagePhase.Campaign.ApplicationUserId,
                FollowUpMessageId = followUpMessage.CampaignProspectFollowUpMessageId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                TimeZoneId = halUnit.TimeZoneId,
                OrderNum = followUpMessage.Order,
                CampaignProspectId = followUpMessage.CampaignProspectId,
                ChromeProfileName = chromeProfileName,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                PageUrl = followUpMessagePhase.PageUrl,
                ProspectName = followUpMessage.CampaignProspect.Name,
                ProspectProfileUrl = followUpMessage.CampaignProspect.ProfileUrl,
                EndOfWorkday = halUnit.EndHour,
                StartOfWorkday = halUnit.StartHour
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("FollowUpMessageBody object is configured with Grid Namespace Name of {gridNamespaceName}", gridNamespaceName);
            _logger.LogTrace("FollowUpMessageBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}", gridServiceDiscoveryName);
            _logger.LogTrace("FollowUpMessageBody object is configured with AppServer Namespace Name of {appServerNamespaceName}", appServerNamespaceName);
            _logger.LogTrace("FollowUpMessageBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}", appServerServiceDiscoveryname);

            return message;
        }
    }
}
