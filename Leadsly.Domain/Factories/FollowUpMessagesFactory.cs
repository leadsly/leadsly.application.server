using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
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
            IHalRepository halRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            ICloudPlatformRepository cloudPlatformRepository
            )
        {
            _cloudPlatformRepository = cloudPlatformRepository;
            _virtualAssistantRepository = virtualAssistantRepository;
            _logger = logger;
            _halRepository = halRepository;
        }

        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<FollowUpMessagesFactory> _logger;


        public async Task<PublishMessageBody> CreateMQMessageAsync(string halId, CampaignProspectFollowUpMessage followUpMessage, FollowUpMessagePhase phase, CancellationToken ct = default)
        {
            PublishMessageBody message = default;
            try
            {
                message = await CreateMQMessageInternalAsync(halId, followUpMessage, phase, ct);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating follow up message");
            }

            return message;
        }

        private async Task<PublishMessageBody> CreateMQMessageInternalAsync(string halId, CampaignProspectFollowUpMessage followUpMessage, FollowUpMessagePhase phase, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating {0} for mq message broker.", nameof(FollowUpMessageBody));
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);

            if (halUnit == null)
            {
                return null;
            }

            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId, ct);

            if (virtualAssistant == null)
            {
                return null;
            };

            EcsService gridEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == EcsResourcePurpose.Grid);
            EcsService proxyEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == EcsResourcePurpose.Proxy);
            string virtualAssistantId = virtualAssistant.VirtualAssistantId;
            if (gridEcsService == null || proxyEcsService == null)
            {
                throw new Exception($"Ecs services not found for virtual assistant {virtualAssistantId}.");
            }

            if (gridEcsService.CloudMapDiscoveryService == null || proxyEcsService.CloudMapDiscoveryService == null)
            {
                throw new Exception($"Cloud map discovery services not found for virtual assistant {virtualAssistantId}.");
            }

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.FollowUpMessage, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                ChromeProfile profileName = new()
                {
                    CampaignPhaseType = PhaseType.FollowUpMessage,
                    Name = Guid.NewGuid().ToString()
                };
                await _halRepository.CreateChromeProfileAsync(profileName, ct);
                chromeProfileName = profileName.Name;
            }

            _logger.LogDebug("The chrome profile used for PhaseType.FollowUpMessage is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            PublishMessageBody message = new FollowUpMessageBody
            {
                ProxyServiceDiscoveryName = proxyEcsService.CloudMapDiscoveryService.Name,
                ProxyNamespaceName = config.ServiceDiscoveryConfig.Proxy.Name,
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                HalId = halId,
                Content = followUpMessage.Content,
                UserId = virtualAssistant.ApplicationUserId,
                FollowUpMessageId = followUpMessage.CampaignProspectFollowUpMessageId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                TimeZoneId = halUnit.TimeZoneId,
                OrderNum = followUpMessage.Order,
                CampaignProspectId = followUpMessage.CampaignProspectId,
                ChromeProfileName = chromeProfileName,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                PageUrl = phase.PageUrl,
                ProspectName = followUpMessage.CampaignProspect.Name,
                ProspectProfileUrl = followUpMessage.CampaignProspect.ProfileUrl,
                EndOfWorkday = halUnit.EndHour,
                StartOfWorkday = halUnit.StartHour,
                ExpectedDeliveryDateTime = followUpMessage.ExpectedDeliveryDateTime,
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("FollowUpMessageBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is {halId}", gridNamespaceName, halId);
            _logger.LogTrace("FollowUpMessageBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("FollowUpMessageBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("FollowUpMessageBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is {halId}", appServerServiceDiscoveryname, halId);

            return message;
        }

    }
}
