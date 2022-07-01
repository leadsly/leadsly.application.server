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
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan);

            if (string.IsNullOrEmpty(command.HalId) == false)
            {
                await InternalHandleAsync(command.HalId, queueNameIn, routingKeyIn, headers);
            }
            else
            {
                await InternalHandleAsync(queueNameIn, routingKeyIn, headers);
            }
        }

        private async Task InternalHandleAsync(string halId, string queueNameIn, string routingKeyIn, Dictionary<string, object> headers)
        {
            MonitorForNewAcceptedConnectionsBody messageBody = await _messagesFactory.CreateMessageAsync(halId, 12);
            if(messageBody != null)
            {
                _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
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
