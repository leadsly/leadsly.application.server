using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Factories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsCommandHandler : ICommandHandler<MonitorForNewConnectionsCommand>
    {
        public MonitorForNewConnectionsCommandHandler(
            IMonitorForNewConnectionsMessagesFactory messagesFactory,
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<MonitorForNewConnectionsCommandHandler> logger
            )
        {
            _messagesFactory = messagesFactory;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly IMonitorForNewConnectionsMessagesFactory _messagesFactory;
        private readonly ILogger<MonitorForNewConnectionsCommandHandler> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        public async Task HandleAsync(MonitorForNewConnectionsCommand command)
        {
            string halId = command.HalId;
            _logger.LogInformation("[HandleAsync] Exuecting MonitorForNewConnectionsCommand for halId: {halId}", halId);
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            _logger.LogInformation($"Setting rabbitMQ headers. Header key {RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType} header value is {RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase}");
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase);

            _logger.LogDebug("Creating MonitorForNewAcceptedConnectionsBody message.");
            MonitorForNewAcceptedConnectionsBody messageBody = await _messagesFactory.CreateMessageAsync(halId, System.Threading.CancellationToken.None);
            if (messageBody != null)
            {
                _logger.LogInformation("Publishing MonitorForNewAcceptedConnectionsBody message to hal {halId}", halId);
                _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
            }
            else
            {
                _logger.LogInformation("Will not publish MonitorForNewAcceptedConnectionsBody message to hal {halId} because the message is null", halId);
            }
        }
    }
}
