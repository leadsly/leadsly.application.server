using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class ProspectListBaseCommand
    {
        public ProspectListBaseCommand(
            ILogger logger, 
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _halRepository = halRepository;
            _timestampService = timestampService;
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
        }

        private readonly ILogger _logger;
        private ICampaignRepositoryFacade _campaignRepositoryFacade;
        private IHalRepository _halRepository;
        private ITimestampService _timestampService;
        private readonly IRabbitMQProvider _rabbitMQProvider;

        protected async Task<ProspectListBody> CreateProspectListBodyAsync(string prospectListPhaseId, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating prospect list body message for rabbit mq message broker.");
            ProspectListPhase prospectListPhase = await _campaignRepositoryFacade.GetProspectListPhaseByIdAsync(prospectListPhaseId, ct);
            string primaryProspectListId = prospectListPhase.Campaign.CampaignProspectList.PrimaryProspectListId;
            _logger.LogDebug("Creating prospect list body with primary prospect list id {primaryProspectListId}", primaryProspectListId);
            string campaignId = prospectListPhase.CampaignId;
            _logger.LogDebug("Creating prospect list body with campaign id {campaignId}", campaignId);

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
                SearchUrls = prospectListPhase.SearchUrls,
                HalId = prospectListPhase.Campaign.HalId,
                ChromeProfile = chromeProfileName,
                PrimaryProspectListId = primaryProspectListId,
                UserId = userId,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("ProspectListBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("ProspectListBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return prospectListBody;
        }
    }
}
