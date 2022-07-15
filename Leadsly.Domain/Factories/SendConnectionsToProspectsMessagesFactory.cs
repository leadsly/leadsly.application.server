using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class SendConnectionsToProspectsMessagesFactory : ISendConnectionsToProspectsMessagesFactory
    {
        public SendConnectionsToProspectsMessagesFactory(
            ILogger<SendConnectionsToProspectsMessagesFactory> logger,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _halRepository = halRepository;
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
        }

        private readonly IHalRepository _halRepository;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly ILogger<SendConnectionsToProspectsMessagesFactory> _logger;

        public async Task<SendConnectionsBody> CreateMessageAsync(Campaign activeCampaign, CampaignWarmUp campaignWarmUp = null, CancellationToken ct = default)
        {
            return await CreateSendConnectionsBodyAsync(activeCampaign, campaignWarmUp, ct);
        }

        private async Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(Campaign campaign, CampaignWarmUp campaignWarmUp = null, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            HalUnit halUnit = await _halRepository.GetByHalIdAsync(campaign.HalId, ct);

            int dailyConnectionsLimit = campaign.DailyInvites;
            _logger.LogDebug("Daily connection request limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            if (campaign.IsWarmUpEnabled == true)
            {
                _logger.LogDebug("Warm up is enabled. Retrieving warm up object from the database");
                dailyConnectionsLimit = campaignWarmUp.DailyLimit;
                _logger.LogDebug("Daily connection has been updated because warm up is enabled. Current daily warm up limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            }

            ChromeProfile chromeProfile = await _halRepository.GetChromeProfileAsync(PhaseType.SendConnectionRequests, ct);
            string chromeProfileName = chromeProfile?.Name;
            if (chromeProfileName == null)
            {
                chromeProfileName = await _rabbitMQProvider.CreateNewChromeProfileAsync(PhaseType.SendConnectionRequests, ct);
            }
            _logger.LogDebug("The chrome profile used for PhaseType.SendConnectionRequests is {chromeProfileName}", chromeProfileName);

            CloudPlatformConfiguration config = _rabbitMQProvider.GetCloudPlatformConfiguration();

            SendConnectionsBody sendConnectionsBody = new()
            {
                ChromeProfileName = chromeProfileName,
                DailyLimit = dailyConnectionsLimit,
                HalId = halUnit.HalId,
                UserId = campaign.ApplicationUserId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour,
                StartDateTimestamp = campaign.StartTimestamp,
                CampaignId = campaign.CampaignId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("SendConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("SendConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return sendConnectionsBody;
        }

        public IList<SendConnectionsStageBody> CreateStages(IList<SendConnectionsStage> sendConnectionsStages, int dailyConnectionsLimit, CancellationToken ct = default)
        {
            return GetSendConnectionsStages(sendConnectionsStages, dailyConnectionsLimit, ct);
        }

        private IList<SendConnectionsStageBody> GetSendConnectionsStages(IList<SendConnectionsStage> sendConnectionsStages, int dailyConnectionsLimit, CancellationToken ct = default)
        {
            _logger.LogInformation("Getting the number of connections each stage should send to stay within the daily limit");
            IList<SendConnectionsStageBody> sendConnectionsStagesBody = new List<SendConnectionsStageBody>();

            decimal totalInvites = dailyConnectionsLimit;
            int divider = sendConnectionsStages.Count;
            List<int> stagesConnectionsLimit = new();
            while (divider > 0)
            {
                decimal stageLimits = Math.Round(totalInvites / sendConnectionsStages.Count, 0);
                int currentStageLimits = Convert.ToInt32(stageLimits);
                stagesConnectionsLimit.Add(currentStageLimits);
                divider--;
            }

            int numOfStages = sendConnectionsStages.Count;
            _logger.LogDebug("Number of send connection stages for this campaign is {numOfStages}", numOfStages);
            for (int i = 0; i < numOfStages; i++)
            {
                int order = sendConnectionsStages[i].Order;
                int stageConnectionsLimit = stagesConnectionsLimit[i];

                SendConnectionsStageBody sendConnectionsStageBody = new()
                {
                    ConnectionsLimit = stageConnectionsLimit,
                    StartTime = sendConnectionsStages[i].StartTime,
                    Order = order
                };

                _logger.LogDebug("This send connections stage has the order of {order}. The limit is {stageConnectionsLimit}", order, stageConnectionsLimit);

                sendConnectionsStagesBody.Add(sendConnectionsStageBody);
            }

            return sendConnectionsStagesBody;
        }
    }
}
