using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessage
{
    public class FollowUpMessageCommandHandler : ICommandHandler<FollowUpMessageCommand>
    {
        public FollowUpMessageCommandHandler(
            IFollowUpMessagesFactory messagesFactory,
            IMessageBrokerOutlet messageBrokerOutlet,
            ISendFollowUpMessageProvider sendFollowUpMessagProvider,
            ILogger<FollowUpMessageCommandHandler> logger
            )
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _messagesFactory = messagesFactory;
            _sendFollowUpMessageProvider = sendFollowUpMessagProvider;
            _logger = logger;
        }

        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IFollowUpMessagesFactory _messagesFactory;
        private readonly ILogger<FollowUpMessageCommandHandler> _logger;
        private readonly ISendFollowUpMessageProvider _sendFollowUpMessageProvider;

        /// <summary>
        /// FollowUpMessage triggered by MonitorForNewProspectsPhase. If new prospect accepts our connection invite and the follow up message delay falls during Hal's work day
        /// This method is triggered to send out the message to that specific campaign prospect. The campaign prospect is found by the campaignProspectFollowUpMessageId parameter
        /// </summary>
        /// <param name="campaignProspectFollowUpMessageId"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task HandleAsync(FollowUpMessageCommand command)
        {
            FollowUpMessageBody followUpMessageBody = await _messagesFactory.CreateMessageAsync(command.CampaignProspectFollowUpMessageId, command.CampaignId);

            string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
            string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
            string halId = followUpMessageBody.HalId;

            if (command.ScheduleTime == default)
            {
                _messageBrokerOutlet.PublishPhase(followUpMessageBody, queueNameIn, routingKeyIn, halId, null);
            }
            else
            {
                _logger.LogInformation($"Scheduling FollowUpMessageBody to go out at {command.ScheduleTime}");
                await _sendFollowUpMessageProvider.ScheduleFollowUpMessageAsync(followUpMessageBody, queueNameIn, routingKeyIn, halId, command.ScheduleTime);

            }
        }
    }
}
