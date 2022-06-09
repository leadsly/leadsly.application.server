using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Campaigns.Handlers;
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

            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = await _messagesFactory.CreateMessagesAsync(12);
            foreach (MonitorForNewAcceptedConnectionsBody body in messageBodies)
            {
                string halId = body.HalId;
                _messageBrokerOutlet.PublishPhase(body, queueNameIn, routingKeyIn, halId, headers);
            }        
        }
    }
}
