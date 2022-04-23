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

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesCommandHandlerBase
    {
        public ScanProspectsForRepliesCommandHandlerBase(
            ILogger logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IRabbitMQProvider rabbitMQProvider,
            IHalRepository halRepository,
            ITimestampService timestampService
            )
        {
            _logger = logger;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _rabbitMQProvider = rabbitMQProvider;
            _halRepository = halRepository;
            _timestampService = timestampService;
        }

        private readonly ILogger _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanProspectsForRepliesPhaseId">Used by the DeepScanProspectsForRepliesPhase</param>
        /// <param name="halId"></param>
        /// <param name="contactedCampaignProspects"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected async Task<ScanProspectsForRepliesBody> CreateScanProspectsForRepliesBodyAsync(string scanProspectsForRepliesPhaseId, string halId, IList<CampaignProspect> contactedCampaignProspects = default, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating scanprospects for replies body message for rabbit mq message broker.");
            ScanProspectsForRepliesPhase scanProspectsForRepliesPhase = await _campaignRepositoryFacade.GetScanProspectsForRepliesPhaseByIdAsync(scanProspectsForRepliesPhaseId, ct);

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.ScanForReplies, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.ScanForReplies, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.ScanForReplies is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            ScanProspectsForRepliesBody scanProspectsForRepliesBody = new()
            {
                PageUrl = scanProspectsForRepliesPhase.PageUrl,
                HalId = halId,
                ChromeProfileName = chromeProfileName,
                UserId = scanProspectsForRepliesPhase.SocialAccount.UserId,
                EndWorkTime = await _timestampService.GetEndWorkDayTimestampAsync(halId),
                TimeZoneId = "Eastern Standard Time",
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            if (contactedCampaignProspects != null)
            {
                scanProspectsForRepliesBody.ContactedCampaignProspects = contactedCampaignProspects;
            }

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("ProspectListBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("ProspectListBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return scanProspectsForRepliesBody;
        }
    }
}
