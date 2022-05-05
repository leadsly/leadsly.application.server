using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
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

namespace Leadsly.Domain.Campaigns.ProspectListsHandlers
{
    public class ProspectListCommandHandlerBase
    {
        public ProspectListCommandHandlerBase(
            ILogger logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _halRepository = halRepository;            
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
        }

        private readonly ILogger _logger;
        private ICampaignRepositoryFacade _campaignRepositoryFacade;
        private IHalRepository _halRepository;        
        private readonly IRabbitMQProvider _rabbitMQProvider;

        protected async Task<ProspectListBody> CreateProspectListBodyAsync(string prospectListPhaseId, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating prospect list body message for rabbit mq message broker.");
            ProspectListPhase prospectListPhase = await _campaignRepositoryFacade.GetProspectListPhaseByIdAsync(prospectListPhaseId, ct);
            string primaryProspectListId = prospectListPhase.Campaign.CampaignProspectList.PrimaryProspectListId;

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(prospectListPhase.Campaign.HalId, ct);

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
                SearchUrls = prospectListPhase.SearchUrls,
                HalId = halUnit.HalId,
                TimeZoneId = halUnit.TimeZoneId,
                EndOfWorkday = halUnit.EndHour,
                StartOfWorkday = halUnit.StartHour,
                ChromeProfile = chromeProfileName,
                PrimaryProspectListId = primaryProspectListId,
                UserId = userId,
                CampaignProspectListId = campaign.CampaignProspectList.CampaignProspectListId,
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
