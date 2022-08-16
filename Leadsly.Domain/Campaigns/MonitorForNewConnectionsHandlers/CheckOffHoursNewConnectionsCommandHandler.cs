using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Factories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class CheckOffHoursNewConnectionsCommandHandler : ICommandHandler<CheckOffHoursNewConnectionsCommand>
    {
        public CheckOffHoursNewConnectionsCommandHandler(
            IMonitorForNewConnectionsMessagesFactory messagesFactory,
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<CheckOffHoursNewConnectionsCommandHandler> logger)
        {
            _messagesFactory = messagesFactory;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly ILogger<CheckOffHoursNewConnectionsCommandHandler> _logger;
        private readonly IMonitorForNewConnectionsMessagesFactory _messagesFactory;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        public async Task HandleAsync(CheckOffHoursNewConnectionsCommand command)
        {
            string halId = command.HalId;
            _logger.LogInformation("[HandleAsync] Exuecting CheckOffHoursNewConnectionsCommand for halId: {halId}", halId);
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            _logger.LogInformation($"Setting rabbitMQ headers. Header key {RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType} header value is {RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan}");
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan);

            if (string.IsNullOrEmpty(command.HalId) == false)
            {
                _logger.LogDebug("HalId is set on the command");
                await InternalHandleAsync(command.HalId, queueNameIn, routingKeyIn, headers);
            }
            else
            {
                _logger.LogDebug("HalId is not set on the command");
                await InternalHandleAsync(queueNameIn, routingKeyIn, headers);
            }
        }

        private async Task InternalHandleAsync(string halId, string queueNameIn, string routingKeyIn, Dictionary<string, object> headers)
        {
            _logger.LogDebug("Creating CheckOffHoursNewConnectionsBody message. The message is configured to check for connections created within last 12 hours");
            MonitorForNewAcceptedConnectionsBody messageBody = await _messagesFactory.CreateMessageAsync(halId, 12);
            if (messageBody != null)
            {
                _logger.LogInformation("Publishing CheckOffHoursNewConnectionsBody message to hal {halId}", halId);
                _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
            }
            else
            {
                _logger.LogInformation("Will not publish CheckOffHoursNewConnectionsBody message to hal {halId} because the created message is null", halId);
            }
        }

        private async Task InternalHandleAsync(string queueNameIn, string routingKeyIn, Dictionary<string, object> headers)
        {
            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = await _messagesFactory.CreateMessagesAsync(12);
            foreach (MonitorForNewAcceptedConnectionsBody body in messageBodies)
            {
                string halId = body.HalId;
                _messageBrokerOutlet.PublishPhase(body, queueNameIn, routingKeyIn, halId, headers);
            }
        }
    }
}
