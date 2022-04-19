using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
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
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class UncontactedFollowUpMessageCommand : FollowUpMessageBaseCommand, ICommand
    {
        public UncontactedFollowUpMessageCommand(
            IMessageBrokerOutlet messageBrokerOutlet, 
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ILogger<UncontactedFollowUpMessageCommand> logger,
            IHalRepository halRepository,
            ISendFollowUpMessageProvider sendFollowUpMessageProvider,
            IRabbitMQProvider rabbitMQProvider
            )
            : base(messageBrokerOutlet, logger, campaignRepositoryFacade, halRepository, rabbitMQProvider)
        {
            _sendFollowUpMessageProvider = sendFollowUpMessageProvider;            
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly ILogger<UncontactedFollowUpMessageCommand> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ISendFollowUpMessageProvider _sendFollowUpMessageProvider;        
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;

        /// <summary>
        /// Triggered every morning and is meant to go out to those prospects who have accepted our invite, who have NOT replied and have NOT gotten a follow up message.
        /// Scenario: On Monday at 6:45 PM, we get a new connection that accepts our request. The follow up message is set up to go out 5 hours after the connection is created.
        /// Hal work day ends at 7:00 PM on Monday. This means that Monday 6:45 PM plus 5 hours = 11:45 PM. The message falls outside of Hal's work day and will not be triggered Monday.
        /// Tuesday morning, this phase is triggered and it checks for all campaign prospects that are part of active campaign that have accepted our invite (our prospect has),
        /// who have NOT received a follow up message (our prospect has not) and who have not replied (naturally our prospect has not replied since they did not have any messages to respond to).
        /// We then check to see if the follow up message delay (5 hours) plus connection accepted date time (Monday 6:45 PM) falls BEFORE Tuesday 7:00AM. If it does we fire the follow up message
        /// right away. If it falls sometime during the work day, we schedule it for that time, if it falls after the work day we do nothing and let the next recurring job take care of it.
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            // first grab all active campaigns
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsAsync();

            IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();
            foreach (Campaign activeCampaign in campaigns)
            {
                // grab all campaign prospects for each campaign
                IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                // filter the list down to only those campaign prospects who have not yet been contacted
                List<CampaignProspect> uncontactedProspects = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpMessageSent == false).ToList();

                IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesOut = await _sendFollowUpMessageProvider.CreateSendFollowUpMessagesAsync(uncontactedProspects);
                messagesGoingOut.Concat(messagesOut);
            }

            await PublishMessagesGoingOut(messagesGoingOut);
        }

        private async Task PublishMessagesGoingOut(IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut)
        {
            foreach (var messagePair in messagesGoingOut)
            {
                FollowUpMessageBody followUpMessageBody = await CreateFollowUpMessageBodyAsync(messagePair.Key.CampaignProspectFollowUpMessageId, messagePair.Key.CampaignProspect.CampaignId);
                string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
                string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
                string halId = followUpMessageBody.HalId;

                if (messagePair.Value == default)
                {
                    _messageBrokerOutlet.PublishPhase(followUpMessageBody, queueNameIn, routingKeyIn, halId, null);
                }
                else
                {
                    BackgroundJob.Schedule<IMessageBrokerOutlet>(x => x.PublishPhase(followUpMessageBody, queueNameIn, routingKeyIn, halId, null), messagePair.Value);
                }
            }
        }
    }
}
