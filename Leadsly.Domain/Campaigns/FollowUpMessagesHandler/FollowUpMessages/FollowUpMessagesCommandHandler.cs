﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessages
{
    public class FollowUpMessagesCommandHandler : ICommandHandler<FollowUpMessagesCommand>
    {
        public FollowUpMessagesCommandHandler(
            IFollowUpMessagesFactory messagesFactory,
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<FollowUpMessagesCommandHandler> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ISendFollowUpMessageProvider sendFollowUpMessagProvider
            )
        {
            _logger = logger;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _messagesFactory = messagesFactory;
            _messageBrokerOutlet = messageBrokerOutlet;
            _sendFollowUpMessageProvider = sendFollowUpMessagProvider;
        }

        private readonly IFollowUpMessagesFactory _messagesFactory;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<FollowUpMessagesCommandHandler> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ISendFollowUpMessageProvider _sendFollowUpMessageProvider;


        /// <summary>
        /// FollowUpMessagePhase triggered after DeepScanProspectsForRepliesPhase is finished running or if DeepScanProspectsForRepliesPhase did not run this is executed
        /// directly        
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task HandleAsync(FollowUpMessagesCommand command)
        {
            if (command.HalId != null)
            {
                string halId = command.HalId;
                _logger.LogInformation("[PublishHalPhasesAsync] Exuecting FollowUpMessagesCommand for halId: {halId}", halId);
                await InternalHandleAsync(halId);
            }

            if (command.HalIds != null)
            {
                await InternalExecuteListAsync(command.HalIds);
            }
        }

        private async Task InternalHandleAsync(string halId)
        {
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);
            string campaignCount = campaigns?.Count == null ? "0" : campaigns.Count.ToString();
            _logger.LogInformation($"[FollowUpMessagesCommandHandler] HalId {halId} has {campaignCount} active campaigns");

            Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();
            foreach (Campaign activeCampaign in campaigns)
            {
                // grab all campaign prospects for each campaign
                IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                List<CampaignProspect> prospectsForFollowUpMessage = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpComplete == false).ToList();
                if (prospectsForFollowUpMessage.Count > 0)
                {
                    _logger.LogInformation("Prospects that need to receive follow up messages is greater than 0 and we will be generating CampaignProspectFollowUpMessage");
                    // await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                    Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesOut = await _sendFollowUpMessageProvider.CreateSendFollowUpMessagesAsync(prospectsForFollowUpMessage) as Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>;
                    messagesGoingOut.AddRange(messagesOut);
                }
                else
                {
                    _logger.LogInformation("Prospects that need to receive follow up messages is 0. No CampaignProspectFollowUpMessage messages will be generated.");
                }
            }

            await PublishMessagesGoingOut(messagesGoingOut);
        }

        /// <summary>
        /// Only triggered for those hals that did NOT have DeepScanProspectsForRepliesPhase executed. DeepScanProspectsForRepliesPhase
        /// will handle execution of FollowUpMessagesPhase and ScanProspectsForReplies, however
        /// it is possible that campaign prospects for specific hal do not return any results for DeepScanProspectsForRepliesPhase to execute,
        /// in such cases we need to directly trigger FollowUpMessagesPhase and ScanProspectsForRepliesPhase
        /// </summary>
        /// <param name="halIds"></param>
        /// <returns></returns>
        private async Task InternalExecuteListAsync(IList<string> halIds)
        {
            foreach (string halId in halIds)
            {
                await InternalHandleAsync(halId);
            }
        }

        private async Task PublishMessagesGoingOut(IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut)
        {
            // sort the messages going out by order
            foreach (var messagePair in messagesGoingOut.OrderBy(k => k.Key.Order))
            {
                FollowUpMessageBody followUpMessageBody = await _messagesFactory.CreateMessageAsync(messagePair.Key.CampaignProspectFollowUpMessageId, messagePair.Key.CampaignProspect.CampaignId);
                await PublishMessage(followUpMessageBody, messagePair.Value);
            }
        }

        private async Task PublishMessage(FollowUpMessageBody followUpMessageBody, DateTimeOffset scheduleTime)
        {
            string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
            string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
            string halId = followUpMessageBody.HalId;

            if (scheduleTime == default)
            {
                _logger.LogInformation("FollowUpMessageBody does not have a schedule time set, sending message immediately");
                _messageBrokerOutlet.PublishPhase(followUpMessageBody, queueNameIn, routingKeyIn, halId, null);
            }
            else
            {
                _logger.LogInformation($"Scheduling FollowUpMessageBody to go out at {scheduleTime}");
                await _sendFollowUpMessageProvider.ScheduleFollowUpMessageAsync(followUpMessageBody, queueNameIn, routingKeyIn, halId, scheduleTime);
            }
        }
    }
}
