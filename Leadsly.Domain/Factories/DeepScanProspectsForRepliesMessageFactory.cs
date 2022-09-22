using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models;
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
    public class DeepScanProspectsForRepliesMessageFactory : IDeepScanProspectsForRepliesMessageFactory
    {
        public DeepScanProspectsForRepliesMessageFactory(
            ILogger<DeepScanProspectsForRepliesMessageFactory> logger,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider,
            ICloudPlatformRepository cloudPlatformRepository)
        {
            _logger = logger;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _cloudPlatformRepository = cloudPlatformRepository;
        }

        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly ILogger<DeepScanProspectsForRepliesMessageFactory> _logger;
        private readonly IHalRepository _halRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;

        public async Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, VirtualAssistant virtualAssistant, ScanProspectsForRepliesPhase phase, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating {0} for mq message broker.", nameof(DeepScanProspectsForRepliesBody));

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);
            if (halUnit == null)
            {
                return null;
            }

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

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.ScanForReplies, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                ChromeProfile profileName = new()
                {
                    CampaignPhaseType = PhaseType.ScanForReplies,
                    Name = Guid.NewGuid().ToString()
                };
                await _halRepository.CreateChromeProfileAsync(profileName, ct);
                chromeProfileName = profileName.Name;
            }
            _logger.LogDebug("The chrome profile used for PhaseType.ScanForReplies is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            DeepScanProspectsForRepliesBody deepScanProspectsForRepliesBody = new()
            {
                ProxyServiceDiscoveryName = proxyEcsService.CloudMapDiscoveryService.Name,
                ProxyNamespaceName = config.ServiceDiscoveryConfig.Proxy.Name,
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                PageUrl = phase.PageUrl,
                HalId = halId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour,
                ChromeProfileName = chromeProfileName,
                UserId = userId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is: {halId}", gridNamespaceName, halId);
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is  {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is  {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("DeepScanProspectsForRepliesBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is  {halId}", appServerServiceDiscoveryname, halId);

            return deepScanProspectsForRepliesBody;
        }
    }
}
