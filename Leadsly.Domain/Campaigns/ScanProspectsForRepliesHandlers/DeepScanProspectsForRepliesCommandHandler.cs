using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class DeepScanProspectsForRepliesCommandHandler : ICommandHandler<DeepScanProspectsForRepliesCommand>
    {
        public DeepScanProspectsForRepliesCommandHandler(
            IScanProspectsForRepliesMessagesFactory messagesFactory,
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<DeepScanProspectsForRepliesCommandHandler> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade
            )
        {
            _messagesFactory = messagesFactory;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IScanProspectsForRepliesMessagesFactory _messagesFactory;
        private readonly ILogger<DeepScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// Triggered on recurring basis once a day. The purpose of this phase is to perform a deep analysis of the conversation history with any campaign prospect to who meets the following conditions
        /// Has accepted our connection request, has gotten a follow up message and has NOT yet replied to our message. This ensures that we can campture responses from campaign prospects
        /// even if leadsly user has communicated with the prospect themselves. Once Hal is finished with this phase, it will send a request to the application server to first trigger 
        /// FollowUpMessagePhase and then ScanProspectsForRepliesPhase
        /// </summary>
        /// <returns></returns>
        public async Task HandleAsync(DeepScanProspectsForRepliesCommand command)
        {
            if (command.HalIds != null)
            {
                _logger.LogInformation("[PublishHalPhasesAsync] Exuecting DeepScanProspectsForRepliesCommand for multiple halIds");
                await InternalExecuteAsync(command.HalIds);
            }
            else if (string.IsNullOrEmpty(command.HalId) == false)
            {
                string halId = command.HalId;
                _logger.LogInformation("[PublishHalPhasesAsync] Exuecting DeepScanProspectsForRepliesCommand for halId: {halId}", halId);
                await InternalExecuteAsync(halId);
            }
        }

        /// <summary>
        /// TODO: this should be converted to only work with single hal id at a time.
        /// </summary>
        /// <param name="halId"></param>
        /// <returns></returns>
        private async Task InternalExecuteAsync(string halId)
        {
            IList<string> halIds = new List<string>()
            {
                halId
            };

            await InternalExecuteAsync(halIds);
        }

        private async Task InternalExecuteAsync(IList<string> halIds)
        {
            foreach (string halId in halIds)
            {
                // fire off DeepScanProspectsForRepliesPhase with the payload of the contacted prospects
                _logger.LogInformation("Creating DeepScanProspectsForRepliesMessage for halId: {halId}", halId);
                DeepScanProspectsForRepliesBody messageBody = await _messagesFactory.CreateDeepScanMessageAsync(halId);

                if (messageBody != null)
                {
                    _logger.LogInformation("Publishing DeepScanProspectsForRepliesMessage for halId: {halId}", halId);
                    PublishMessage(messageBody);
                }
                else
                {
                    _logger.LogInformation("No DeepScanProspectsForRepliesMessage to publish for halId: {halId} because DeepScanProspectsForRepliesMessage was not generated.", halId);
                }
            }
        }

        private void PublishMessage(DeepScanProspectsForRepliesBody messageBody)
        {
            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = messageBody.HalId;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
