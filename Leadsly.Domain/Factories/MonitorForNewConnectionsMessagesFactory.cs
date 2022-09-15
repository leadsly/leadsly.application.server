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
    public class MonitorForNewConnectionsMessagesFactory : IMonitorForNewConnectionsMessagesFactory
    {
        public MonitorForNewConnectionsMessagesFactory(
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            ILogger<MonitorForNewConnectionsMessagesFactory> logger)
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _logger = logger;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<MonitorForNewConnectionsMessagesFactory> _logger;

        public async Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, Models.Entities.VirtualAssistant virtualAssistant, MonitorForNewConnectionsPhase phase, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating {0} for mq message broker.", nameof(MonitorForNewAcceptedConnectionsBody));

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);
            if (halUnit == null)
            {
                _logger.LogDebug("Failed to get hal unit by halId {halId}", halId);
                return null;
            }

            _logger.LogInformation($"Creating {nameof(MonitorForNewAcceptedConnectionsBody)} MQ message.");
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

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.MonitorNewConnections, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                ChromeProfile profileName = new()
                {
                    CampaignPhaseType = PhaseType.MonitorNewConnections,
                    Name = Guid.NewGuid().ToString()
                };
                await _halRepository.CreateChromeProfileAsync(profileName, ct);
                chromeProfileName = profileName.Name;
            }
            _logger.LogDebug($"The chrome profile used for {nameof(PhaseType.MonitorNewConnections)} is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            PublishMessageBody monitorForNewAcceptedConnectionsBody = new MonitorForNewAcceptedConnectionsBody()
            {
                ProxyServiceDiscoveryName = proxyEcsService.CloudMapDiscoveryService.Name,
                ProxyNamespaceName = config.ServiceDiscoveryConfig.Proxy.Name,
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                ChromeProfileName = chromeProfileName,
                HalId = halId,
                UserId = userId,
                TimeZoneId = halUnit.TimeZoneId,
                PageUrl = phase.PageUrl,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is {halId}", gridNamespaceName, halId);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is {halId}", appServerServiceDiscoveryname, halId);

            return monitorForNewAcceptedConnectionsBody;
        }
    }
}
