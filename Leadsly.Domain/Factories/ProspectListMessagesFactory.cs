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
    public class ProspectListMessagesFactory : IProspectListMessagesFactory
    {
        public ProspectListMessagesFactory(
            ILogger<ProspectListMessagesFactory> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _halRepository = halRepository;
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly ILogger<ProspectListMessagesFactory> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IHalRepository _halRepository;
        private readonly IRabbitMQProvider _rabbitMQProvider;

        public async Task<ProspectListBody> CreateMessageAsync(string prospectListPhaseId, string userId, CancellationToken ct = default)
        {
            return await CreateProspectListBodyAsync(prospectListPhaseId, userId, ct);
        }
        private async Task<ProspectListBody> CreateProspectListBodyAsync(string prospectListPhaseId, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating prospect list body message for rabbit mq message broker.");
            ProspectListPhase prospectListPhase = await _campaignRepositoryFacade.GetProspectListPhaseByIdAsync(prospectListPhaseId, ct);
            string primaryProspectListId = prospectListPhase.Campaign.CampaignProspectList.PrimaryProspectListId;

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(prospectListPhase.Campaign.HalId, ct);
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

            _logger.LogDebug("Creating prospect list body with primary prospect list id {primaryProspectListId}", primaryProspectListId);
            string campaignId = prospectListPhase.CampaignId;
            _logger.LogDebug("Creating prospect list body with campaign id {campaignId}", campaignId);
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.ProspectList, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.ProspectList, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.ProspectList is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            ProspectListBody prospectListBody = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                SearchUrls = prospectListPhase.SearchUrls,
                ProspectListPhaseId = prospectListPhase.ProspectListPhaseId,
                //SocialAccountId = halUnit.SocialAccountId,
                HalId = halUnit.HalId,
                TimeZoneId = halUnit.TimeZoneId,
                EndOfWorkday = halUnit.EndHour,
                StartOfWorkday = halUnit.StartHour,
                ChromeProfileName = chromeProfileName,
                PrimaryProspectListId = primaryProspectListId,
                UserId = userId,
                CampaignProspectListId = campaign.CampaignProspectList.CampaignProspectListId,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("ProspectListBody object is configured with Grid Namespace Name of {gridNamespaceName}", gridNamespaceName);
            _logger.LogTrace("ProspectListBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}", gridServiceDiscoveryName);
            _logger.LogTrace("ProspectListBody object is configured with AppServer Namespace Name of {appServerNamespaceName}", appServerNamespaceName);
            _logger.LogTrace("ProspectListBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}", appServerServiceDiscoveryname);

            return prospectListBody;
        }
    }
}
