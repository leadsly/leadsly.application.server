using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
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

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsCommandHandlerBase
    {
        public MonitorForNewConnectionsCommandHandlerBase(
            IUserProvider userProvider,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository,
            ITimestampService timestampService,
            ILogger logger)
        {
            _userProvider = userProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _timestampService = timestampService;
            _logger = logger;
        }

        private readonly IUserProvider _userProvider;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;
        private readonly ILogger _logger;

        protected async Task<IList<MonitorForNewAcceptedConnectionsBody>> CreateMessageBodiesAsync(int numOfHoursAgo = 0)
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

        protected async Task<MonitorForNewAcceptedConnectionsBody> CreateMessageBodyAsync(string halId)
        {
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId);
            SocialAccount socialAccount = await _userProvider.GetSocialAccountByHalIdAsync(halId);

            MonitorForNewAcceptedConnectionsBody messageBody = await CreateMonitorForNewAcceptedConnectionsBodyAsync(socialAccount.HalDetails.HalId, socialAccount.UserId, socialAccount.SocialAccountId);
            return messageBody;
        }

        private async Task<MonitorForNewAcceptedConnectionsBody> CreateMonitorForNewAcceptedConnectionsBodyAsync(string halId, string userId, string socialAccountId, int numOfHoursAgo = 0, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating monitor for new connections body message for rabbit mq message broker.");
            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = await _campaignRepositoryFacade.GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(socialAccountId, ct);
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId, ct);

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
                TimeZoneId = halUnit.TimeZoneId,
                PageUrl = monitorForNewConnectionsPhase.PageUrl,
                StartWorkTime = await _timestampService.GetStartWorkDayTimestampAsync(halId),
                EndWorkTime = await _timestampService.GetEndWorkDayTimestampAsync(halId),
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                NumOfHoursAgo = numOfHoursAgo,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("MonitorForNewAcceptedConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return monitorForNewAcceptedConnectionsBody;
        }
    }
}
