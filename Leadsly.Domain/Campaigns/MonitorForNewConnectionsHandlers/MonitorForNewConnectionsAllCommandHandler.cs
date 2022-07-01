using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
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

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsAllCommandHandler : ICommandHandler<MonitorForNewConnectionsAllCommand>
    {
        public MonitorForNewConnectionsAllCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            IMonitorForNewConnectionsMessagesFactory messagesFactory,
            ILogger<MonitorForNewConnectionsAllCommandHandler> logger)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _messagesFactory = messagesFactory;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsAllCommandHandler> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IMonitorForNewConnectionsMessagesFactory _messagesFactory;

        public async Task HandleAsync(MonitorForNewConnectionsAllCommand command)
        {
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase);

            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = await _messagesFactory.CreateMessagesAsync();
            foreach (MonitorForNewAcceptedConnectionsBody body in messageBodies)
            {
                string halId = body.HalId;
                _messageBrokerOutlet.PublishPhase(body, queueNameIn, routingKeyIn, halId, headers);
            }
        }
    }
}
