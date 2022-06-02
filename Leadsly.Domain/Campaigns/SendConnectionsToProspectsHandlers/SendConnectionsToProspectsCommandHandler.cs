using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
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
            ISendConnectionsToProspectsMessagesFactory messagesFactory,
            ILogger<SendConnectionsToProspectsCommandHandler> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ITimestampService timestampService)            
        {
            _campaignRepositoryFacade = campaignRepositoryFacade; 
            _messagesFactory = messagesFactory;
            _timestampService = timestampService;
            _logger = logger;
        }

        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;  
        private readonly ISendConnectionsToProspectsMessagesFactory _messagesFactory;
        private readonly ITimestampService _timestampService;
        private readonly ILogger<SendConnectionsToProspectsCommandHandler> _logger;

        /// <summary>
        /// Triggered by a new campaign that is using existing ProspectList or when ProspectListPhase finishes and we're ready to send out connections for the given campaign.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task HandleAsync(SendConnectionsToProspectsCommand command)
        {
            // triggered on recurring basis
            if(command.HalIds != null && command.HalIds.Count > 0)
            {
                await HandleInternalAsync(command.HalIds);
            }
            // triggered on new campaign
            else if(command.CampaignId != null && command.UserId != null)
            {
                await HandleInternalAsync(command.CampaignId, command.UserId);
            }
        }

        private async Task HandleInternalAsync(IList<string> halIds)
        {
            foreach (string halId in halIds)
            {
                IList<SendConnectionsBody> bodies = await CreateSendConnectionsBodiesAsync(halId);

                foreach (SendConnectionsBody body in bodies)
                {
                    IList<SendConnectionsStage> sendConnectionStages = await _campaignRepositoryFacade.GetStagesByCampaignIdAsync(body.CampaignId);
                    IList<SendConnectionsStageBody> stages = _messagesFactory.CreateStages(sendConnectionStages, body.DailyLimit);
                    await SchedulePhaseMessagesAsync(body, stages);
                }                
            }
        }

        private async Task HandleInternalAsync(string campaignId, string userId)
        {
            Campaign campaign = await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId);
            CampaignWarmUp campaignWarmUp = await _campaignRepositoryFacade.GetCampaignWarmUpByIdAsync(campaignId);

            SendConnectionsBody messageBody = await _messagesFactory.CreateMessageAsync(campaign, campaignWarmUp);
            IList<SendConnectionsStage> sendConnectionStages = await _campaignRepositoryFacade.GetStagesByCampaignIdAsync(campaignId);
            IList<SendConnectionsStageBody> stages = _messagesFactory.CreateStages(sendConnectionStages, messageBody.DailyLimit);

            await SchedulePhaseMessagesAsync(messageBody, stages);
        }

        private async Task<IList<SendConnectionsBody>> CreateSendConnectionsBodiesAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating send connections body message for rabbit mq message broker.");
            IList<SendConnectionsBody> messageBodies = new List<SendConnectionsBody>();
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);
            foreach (Campaign activeCampaign in campaigns)
            {                
                CampaignWarmUp campaignWarmUp = await _campaignRepositoryFacade.GetCampaignWarmUpByIdAsync(activeCampaign.CampaignId);
                SendConnectionsBody messageBody = await _messagesFactory.CreateMessageAsync(activeCampaign, campaignWarmUp);
                messageBodies.Add(messageBody);
            }

            return messageBodies;
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

            DateTimeOffset nowLocalized = await _timestampService.GetNowLocalizedAsync(halId);
            DateTimeOffset phaseStartDateTimeOffset = await _timestampService.ParseDateTimeOffsetLocalizedAsync(halId, stage.StartTime);            

            if (nowLocalized.TimeOfDay < phaseStartDateTimeOffset.TimeOfDay)
            {
                _logger.LogInformation($"[SendConnectionsHandler]: Publishing message for {phaseStartDateTimeOffset}. Current local time is: {nowLocalized}");
                BackgroundJob.Schedule<IMessageBrokerOutlet>(x => x.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers), phaseStartDateTimeOffset);
            }
            else
            {
                _logger.LogInformation($"[SendConnectionsHandler]: Message will NOT be published because it is in the past {phaseStartDateTimeOffset}. Current local time is: {nowLocalized}");
                // temporary to schedule jobs right away                
                //_messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
            }
        }
    }
}
