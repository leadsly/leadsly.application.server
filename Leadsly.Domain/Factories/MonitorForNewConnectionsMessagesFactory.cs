using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
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
    public class MonitorForNewConnectionsMessagesFactory : IMonitorForNewConnectionsMessagesFactory
    {
        public MonitorForNewConnectionsMessagesFactory(
            IUserProvider userProvider,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            ILogger<MonitorForNewConnectionsMessagesFactory> logger)
        {
            _userProvider = userProvider;
            _virtualAssistantRepository = virtualAssistantRepository;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _logger = logger;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IUserProvider _userProvider;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<MonitorForNewConnectionsMessagesFactory> _logger;

        public async Task<MonitorForNewAcceptedConnectionsBody> CreateMessageAsync(string halId, int numOfHoursAgo, CancellationToken ct = default)
        {
            return await CreateMessageBodyForActiveCampaignsAsync(halId, numOfHoursAgo, ct);
        }

        public async Task<MonitorForNewAcceptedConnectionsBody> CreateMessageAsync(string halId, CancellationToken ct = default)
        {
            return await CreateMessageBodyForActiveCampaignsAsync(halId, 0, ct);
        }

        private async Task<MonitorForNewAcceptedConnectionsBody> CreateMessageBodyForActiveCampaignsAsync(string halId, int numOfHoursAgo, CancellationToken ct = default)
        {
            SocialAccount socialAccount = await _userProvider.GetSocialAccountByHalIdAsync(halId, ct);
            string email = socialAccount.Username;
            _logger.LogDebug("Social account {email} is associated with HalId {halId}", email, halId);
            MonitorForNewAcceptedConnectionsBody messageBody = default;
            if (socialAccount.User.Campaigns.Any(c => c.Active == true))
            {
                _logger.LogDebug("Social account {email} has active campaigns", email);
                messageBody = await CreateMonitorForNewAcceptedConnectionsBodyAsync(halId, socialAccount.UserId, socialAccount.SocialAccountId, numOfHoursAgo);
            }
            else
            {
                _logger.LogDebug("Social account {email} does not have any active campaigns. This means that the message body will not be generated.", email);
            }

            return messageBody;
        }

        public async Task<IList<MonitorForNewAcceptedConnectionsBody>> CreateMessagesAsync(int numOfHoursAgo = 0, CancellationToken ct = default)
        {
            return await CreateMessageBodiesAsync(numOfHoursAgo, ct);
        }

        private async Task<IList<MonitorForNewAcceptedConnectionsBody>> CreateMessageBodiesAsync(int numOfHoursAgo = 0, CancellationToken ct = default)
        {
            IList<SocialAccount> socialAccounts = await _userProvider.GetAllSocialAccounts();
            IList<SocialAccount> socialAccountsWithActiveCampaigns = socialAccounts.Where(s => s.User.Campaigns.Any(c => c.Active == true)).ToList();

            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = new List<MonitorForNewAcceptedConnectionsBody>();
            // for each hal with active campaigns trigger MonitorForNewProspectsPhase
            foreach (SocialAccount socialAccount in socialAccountsWithActiveCampaigns)
            {
                MonitorForNewAcceptedConnectionsBody messageBody = await CreateMonitorForNewAcceptedConnectionsBodyAsync(socialAccount.HalDetails.HalId, socialAccount.UserId, socialAccount.SocialAccountId, numOfHoursAgo);

                messageBodies.Add(messageBody);
            }

            return messageBodies;
        }

        private async Task<MonitorForNewAcceptedConnectionsBody> CreateMonitorForNewAcceptedConnectionsBodyAsync(string halId, string userId, string socialAccountId, int numOfHoursAgo = 0, CancellationToken ct = default)
        {
            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = await _campaignRepositoryFacade.GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(socialAccountId, ct);
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);
            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId, ct);
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

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.MonitorNewConnections, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.MonitorNewConnections, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.MonitorNewConnections is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            MonitorForNewAcceptedConnectionsBody monitorForNewAcceptedConnectionsBody = new()
            {
                GridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name,
                ChromeProfileName = chromeProfileName,
                HalId = halId,
                UserId = userId,
                TimeZoneId = halUnit.TimeZoneId,
                PageUrl = monitorForNewConnectionsPhase.PageUrl,
                NamespaceName = config.ServiceDiscoveryConfig.AppServer.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                NumOfHoursAgo = numOfHoursAgo,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour
            };

            string appServerNamespaceName = config.ServiceDiscoveryConfig.AppServer.Name;
            string appServerServiceDiscoveryname = config.ApiServiceDiscoveryName;
            string gridNamespaceName = config.ServiceDiscoveryConfig.Grid.Name;
            string gridServiceDiscoveryName = gridEcsService.CloudMapDiscoveryService.Name;
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody/CheckOffHoursNewConnectionsBody object is configured with Grid Namespace Name of {gridNamespaceName}. This HalId is {halId}", gridNamespaceName, halId);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody/CheckOffHoursNewConnectionsBody object is configured with Grid Service discovery name of {gridServiceDiscoveryname}. This HalId is {halId}", gridServiceDiscoveryName, halId);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody/CheckOffHoursNewConnectionsBody object is configured with AppServer Namespace Name of {appServerNamespaceName}. This HalId is {halId}", appServerNamespaceName, halId);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody/CheckOffHoursNewConnectionsBody object is configured with AppServer Service discovery name of {appServerServiceDiscoveryname}. This HalId is {halId}", appServerServiceDiscoveryname, halId);

            return monitorForNewAcceptedConnectionsBody;
        }
    }
}
