using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class FollowUpMessagesFactory : IFollowUpMessagesFactory
    {
        public FollowUpMessagesFactory(
            ILogger<FollowUpMessagesFactory> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
            _halRepository = halRepository;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<FollowUpMessagesFactory> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;

        public async Task<FollowUpMessageBody> CreateMessageAsync(string campaignProspectFollowUpMessageId, string campaignId, CancellationToken ct = default)
        {
            return await CreateFollowUpMessageBodyAsync(campaignProspectFollowUpMessageId, campaignId, ct); ;
        }

        private async Task<FollowUpMessageBody> CreateFollowUpMessageBodyAsync(string campaignProspectFollowUpMessageId, string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating follow up message body for rabbit mq message broker.");

            FollowUpMessagePhase followUpMessagePhase = await _campaignRepositoryFacade.GetFollowUpMessagePhaseByCampaignIdAsync(campaignId, ct);
            CampaignProspectFollowUpMessage followUpMessage = await _campaignRepositoryFacade.GetCampaignProspectFollowUpMessageByIdAsync(campaignProspectFollowUpMessageId, ct);
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(followUpMessagePhase.Campaign.HalId);

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.FollwUpMessage, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.FollwUpMessage, ct);
            }

            _logger.LogDebug("The chrome profile used for PhaseType.FollowUpMessage is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            FollowUpMessageBody message = new()
            {
                HalId = followUpMessagePhase.Campaign.HalId,
                Content = followUpMessage.Content,
                UserId = followUpMessagePhase.Campaign.ApplicationUserId,
                FollowUpMessageId = followUpMessage.CampaignProspectFollowUpMessageId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                TimeZoneId = halUnit.TimeZoneId,
                OrderNum = followUpMessage.Order,
                CampaignProspectId = followUpMessage.CampaignProspectId,
                ChromeProfileName = chromeProfileName,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName,
                PageUrl = followUpMessagePhase.PageUrl,
                ProspectName = followUpMessage.CampaignProspect.Name,
                ProspectProfileUrl = followUpMessage.CampaignProspect.ProfileUrl,
                EndOfWorkday = halUnit.EndHour,
                StartOfWorkday = halUnit.StartHour
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("SendConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("SendConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return message;
        }
    }
}
