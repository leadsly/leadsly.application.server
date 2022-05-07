﻿using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Campaigns.Handlers;
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

namespace Leadsly.Domain.Campaigns.SendConnectionsToProspectsHandlers
{
    public class SendConnectionsToProspectsCommandHandler : ICommandHandler<SendConnectionsToProspectsCommand>
    {
        public SendConnectionsToProspectsCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<SendConnectionsToProspectsCommandHandler> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider
            )
        {
            _campaignRepositoryFacade = campaignRepositoryFacade;            
            _halRepository = halRepository;
            _timestampService = timestampService;
            _messageBrokerOutlet = messageBrokerOutlet;
            _rabbitMQProvider = rabbitMQProvider;
            _logger = logger;
        }

        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;        
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IRabbitMQProvider _rabbitMQProvider;
        private readonly ILogger<SendConnectionsToProspectsCommandHandler> _logger;

        /// <summary>
        /// Triggered by a new campaign that is using existing ProspectList or when ProspectListPhase finishes and we're ready to send out connections for the given campaign.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task HandleAsync(SendConnectionsToProspectsCommand command)
        {
            if(command.HalIds != null && command.HalIds.Count > 0)
            {
                await HandleInternalListAsync(command.HalIds);
            }
            else if(command.CampaignId != null && command.UserId != null)
            {
                await HandleInternalAsync(command.CampaignId, command.UserId);
            }
        }

        private async Task HandleInternalListAsync(IList<string> halIds)
        {
            foreach (string halId in halIds)
            {
                IList<SendConnectionsBody> bodies = await CreateSendConnectionsBodiesAsync(halId);

                foreach (SendConnectionsBody body in bodies)
                {
                    IList<SendConnectionsStageBody> stages = await CreateStagesAsync(body);
                    await SchedulePhaseMessagesAsync(body, stages);
                }                
            }
        }

        private async Task HandleInternalAsync(string campaignId, string userId)
        {
            SendConnectionsBody messageBody = await CreateSendConnectionsBodyAsync(campaignId, userId);
            IList<SendConnectionsStageBody> stages = await CreateStagesAsync(messageBody);
            await SchedulePhaseMessagesAsync(messageBody, stages);
        }

        private async Task SchedulePhaseMessagesAsync(SendConnectionsBody messageBody, IList<SendConnectionsStageBody> stages)
        {
            foreach (SendConnectionsStageBody stage in stages)
            {
                await SchedulePhaseMessageAsync(messageBody, stage);
            }
        }

        private async Task SchedulePhaseMessageAsync(SendConnectionsBody messageBody, SendConnectionsStageBody stage)
        {
            string queueNameIn = RabbitMQConstants.NetworkingConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.NetworkingConnections.RoutingKey;
            string halId = messageBody.HalId;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.NetworkingConnections.NetworkingType, RabbitMQConstants.NetworkingConnections.SendConnectionRequests);

            messageBody.SendConnectionsStage = stage;

            // TODO needs to be adjusted for DateTimeOffset and user's timeZoneId
            DateTimeOffset now = await _timestampService.CreateNowDatetimeOffsetAsync(halId);
            if (DateTimeOffset.TryParse(stage.StartTime, out DateTimeOffset phaseStartDateTime) == false)
            {
                string startTime = stage.StartTime;
                _logger.LogError("Failed to parse SendConnectionRequests start time. Tried to parse {startTime}", startTime);
            }

            if (now.TimeOfDay < phaseStartDateTime.TimeOfDay)
            {
                BackgroundJob.Schedule<IMessageBrokerOutlet>(x => x.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers), phaseStartDateTime);
            }
            else
            {
                // temporary to schedule jobs right away                
                _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
            }
        }

        private async Task<IList<SendConnectionsStageBody>> CreateStagesAsync(SendConnectionsBody messageBody)
        {
            string campaignId = messageBody.CampaignId;
            IList<SendConnectionsStageBody> sendConnectionsStagesBody = await GetSendConnectionsStagesAsync(campaignId, messageBody.DailyLimit);

            return sendConnectionsStagesBody;
        }

        private async Task<IList<SendConnectionsStageBody>> GetSendConnectionsStagesAsync(string campaignId, int dailyConnectionsLimit, CancellationToken ct = default)
        {
            _logger.LogInformation("Getting the number of connections each stage should send to stay within the daily limit");
            IList<SendConnectionsStageBody> sendConnectionsStagesBody = new List<SendConnectionsStageBody>();
            IList<SendConnectionsStage> sendConnectionsStages = await _campaignRepositoryFacade.GetStagesByCampaignIdAsync(campaignId, ct);

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

        private async Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(campaign.HalId, ct);

            int dailyConnectionsLimit = campaign.DailyInvites;
            _logger.LogDebug("Daily connection request limit is {dailyConnectionsLimit}", dailyConnectionsLimit);
            if (campaign.IsWarmUpEnabled == true)
            {
                _logger.LogDebug("Warm up is enabled. Retrieving warm up object from the database");
                CampaignWarmUp campaignWarmUp = await _campaignRepositoryFacade.GetCampaignWarmUpByIdAsync(campaignId, ct);
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
                UserId = userId,
                TimeZoneId = halUnit.TimeZoneId,
                StartOfWorkday = halUnit.StartHour,
                EndOfWorkday = halUnit.EndHour, 
                StartDateTimestamp = campaign.StartTimestamp,
                CampaignId = campaignId,
                NamespaceName = config.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = config.ApiServiceDiscoveryName
            };

            string namespaceName = config.ServiceDiscoveryConfig.Name;
            string serviceDiscoveryname = config.ApiServiceDiscoveryName;
            _logger.LogTrace("SendConnectionsBody object is configured with Namespace Name of {namespaceName}", namespaceName);
            _logger.LogTrace("SendConnectionsBody object is configured with Service discovery name of {serviceDiscoveryname}", serviceDiscoveryname);

            return sendConnectionsBody;
        }

        private async Task<IList<SendConnectionsBody>> CreateSendConnectionsBodiesAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            IList<SendConnectionsBody> messageBodies = new List<SendConnectionsBody>();
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);
            foreach (Campaign activeCampaign in campaigns)
            {
                SendConnectionsBody messageBody = await CreateSendConnectionsBodyAsync(activeCampaign.CampaignId, activeCampaign.ApplicationUserId);
                messageBodies.Add(messageBody);
            }

            return messageBodies;
        }
    }
}
