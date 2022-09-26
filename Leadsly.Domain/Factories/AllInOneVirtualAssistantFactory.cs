using Leadsly.Domain.Decorators;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
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
    public class AllInOneVirtualAssistantFactory : IAllInOneVirtualAssistantFactory
    {
        public AllInOneVirtualAssistantFactory(
            ILogger<AllInOneVirtualAssistantFactory> logger,
            VirtualAssistantRepositoryCache virtualAssistantRepository,
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository)
        {
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
            _halRepository = halRepository;
            _virtualAssistantRepository = virtualAssistantRepository;
        }

        private readonly ILogger<AllInOneVirtualAssistantFactory> _logger;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly VirtualAssistantRepositoryCache _virtualAssistantRepository;

        public async Task<PublishMessageBody> CreateMQMessageAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating {0} for mq message broker.", nameof(AllInOneVirtualAssistantMessageBody));
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);
            if (halUnit == null)
            {
                _logger.LogError("Could not find hal id by hal id {halId}", halId);
                return null;
            }

            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId, ct);
            if (virtualAssistant == null)
            {
                _logger.LogError("Could not find virtual assistant by HalId {0}", halId);
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

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            PublishMessageBody mqMessage = new AllInOneVirtualAssistantMessageBody
            {
                EndOfWorkday = halUnit.EndHour,
                StartOfWorkday = halUnit.StartHour,
                ProxyNamespaceName = config.ServiceDiscoveryConfig.Proxy.Name,
                ProxyServiceDiscoveryName = proxyEcsService.CloudMapDiscoveryService.Name,
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                HalId = halId,
                UserId = halUnit.ApplicationUserId,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                TimeZoneId = halUnit.TimeZoneId
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            string proxyNamespaceName = config.ServiceDiscoveryConfig.Proxy.Name;
            string proxyServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("{0} object is configured with Proxy Namespace Name of {1}. This HalId is {2}", nameof(AllInOneVirtualAssistantMessageBody), proxyNamespaceName, halId);
            _logger.LogTrace("{0} object is configured with Proxy Service discovery name of {1}. This HalId is {2}", nameof(AllInOneVirtualAssistantMessageBody), proxyServiceDiscoveryName, halId);
            _logger.LogTrace("{0} object is configured with Grid Namespace Name of {1}. This HalId is {2}", nameof(AllInOneVirtualAssistantMessageBody), gridNamespaceName, halId);
            _logger.LogTrace("{0} object is configured with Grid Service discovery name of {1}. This HalId is {2}", nameof(AllInOneVirtualAssistantMessageBody), gridServiceDiscoveryName, halId);
            _logger.LogTrace("{0} object is configured with AppServer Namespace Name of {1}. This HalId is {2}", nameof(AllInOneVirtualAssistantMessageBody), appServerNamespaceName, halId);
            _logger.LogTrace("{0} object is configured with AppServer Service discovery name of {1}. This HalId is {2}", nameof(AllInOneVirtualAssistantMessageBody), appServerServiceDiscoveryname, halId);

            return mqMessage;
        }
    }
}
