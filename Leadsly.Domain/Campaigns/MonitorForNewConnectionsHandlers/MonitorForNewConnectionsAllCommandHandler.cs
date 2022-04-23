using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandler;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Handlers
{
    public class MonitorForNewConnectionsAllCommandHandler : ICommandHandler<MonitorForNewConnectionsAllCommand>
    {
        public MonitorForNewConnectionsAllCommandHandler(IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<MonitorForNewConnectionsAllCommandHandler> logger,
            IUserProvider userProvider,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider)
        {           
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
            _userProvider = userProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _halRepository = halRepository;
            _timestampService = timestampService;
            _rabbitMQProvider = rabbitMQProvider;
        }

        private readonly ILogger<MonitorForNewConnectionsAllCommandHandler> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;        
        private readonly IUserProvider _userProvider;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;
        private readonly IRabbitMQProvider _rabbitMQProvider;

        public async Task HandleAsync(MonitorForNewConnectionsAllCommand command)
        {
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;

            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = await CreateMessageBodiesAsync();
            foreach (MonitorForNewAcceptedConnectionsBody body in messageBodies)
            {
                string halId = body.HalId;
                _messageBrokerOutlet.PublishPhase(body, queueNameIn, routingKeyIn, halId, null);
            }
        }

        private async Task<IList<MonitorForNewAcceptedConnectionsBody>> CreateMessageBodiesAsync()
        {
            IList<SocialAccount> socialAccounts = await _userProvider.GetAllSocialAccounts();
            IList<SocialAccount> socialAccountsWithActiveCampaigns = socialAccounts.Where(s => s.User.Campaigns.Any(c => c.Active == true)).ToList();

            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = new List<MonitorForNewAcceptedConnectionsBody>();
            // for each hal with active campaigns trigger MonitorForNewProspectsPhase
            foreach (SocialAccount socialAccount in socialAccountsWithActiveCampaigns)
            {
                MonitorForNewAcceptedConnectionsBody messageBody = await CreateMonitorForNewAcceptedConnectionsBodyAsync(socialAccount.HalDetails.HalId, socialAccount.UserId, socialAccount.SocialAccountId);

                messageBodies.Add(messageBody);
            }

            return messageBodies;
        }

        private async Task<MonitorForNewAcceptedConnectionsBody> CreateMonitorForNewAcceptedConnectionsBodyAsync(string halId, string userId, string socialAccountId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating monitor for new connections body message for rabbit mq message broker.");
            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = await _campaignRepositoryFacade.GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(socialAccountId, ct);

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
                ChromeProfileName = chromeProfileName,
                HalId = halId,
                UserId = userId,
                TimeZoneId = "Eastern Standard Time",
                PageUrl = monitorForNewConnectionsPhase.PageUrl,
                StartWorkTime = await _timestampService.GetStartWorkDayTimestampAsync(halId),
                EndWorkTime = await _timestampService.GetEndWorkDayTimestampAsync(halId),
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return monitorForNewAcceptedConnectionsBody;
        }
    }
}
