using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Factories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesCommandHandler : ICommandHandler<ScanProspectsForRepliesCommand>
    {
        public ScanProspectsForRepliesCommandHandler(
            IScanProspectsForRepliesMessagesFactory messagesFactory,
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<ScanProspectsForRepliesCommand> logger
            )
        {
            _messagesFactory = messagesFactory;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly IScanProspectsForRepliesMessagesFactory _messagesFactory;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<ScanProspectsForRepliesCommand> _logger;

        public async Task HandleAsync(ScanProspectsForRepliesCommand command)
        {
            if (command.HalIds != null)
            {
                await InternalHandleAsync(command.HalIds);
            }

            if (command.HalId != null)
            {
                string halId = command.HalId;
                _logger.LogInformation("[PublishHalPhasesAsync] Exuecting ScanProspectsForRepliesCommand for halId: {halId}", halId);
                await InternalHandleAsync(halId);
            }
        }

        private async Task InternalHandleAsync(IList<string> halIds)
        {
            foreach (string halId in halIds)
            {
                await InternalHandleAsync(halId);
            }
        }

        private async Task InternalHandleAsync(string halId)
        {
            ScanProspectsForRepliesBody messageBody = await _messagesFactory.CreateMessageAsync(halId);

            if (messageBody != null)
            {
                string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
                string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;

                Dictionary<string, object> headers = new Dictionary<string, object>();
                headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecutePhase);

                _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
            }
        }
    }
}
