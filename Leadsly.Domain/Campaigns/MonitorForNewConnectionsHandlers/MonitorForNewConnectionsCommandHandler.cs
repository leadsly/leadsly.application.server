﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
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
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            string halId = command.HalId;            

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase);

            MonitorForNewAcceptedConnectionsBody messageBody = await _messagesFactory.CreateMessageAsync(halId);
            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
