using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Campaigns.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsCommandHandler : ICommandHandler<MonitorForNewConnectionsCommand>
    {
        public MonitorForNewConnectionsCommandHandler(IMessageBrokerOutlet messageBrokerOutlet)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        public async Task HandleAsync(MonitorForNewConnectionsCommand command)
        {
            MonitorForNewAcceptedConnectionsBody messageBody = await CreateMessageBodyAsync();

            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            string halId = messageBody.HalId;

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, null);
        }

        /// <summary>
        /// Triggered once a new campaign is created. The intent is to ensure that this phase is running before we execute a new campaign.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Task<MonitorForNewAcceptedConnectionsBody> CreateMessageBodyAsync()
        {
            MonitorForNewAcceptedConnectionsBody messageBody = new MonitorForNewAcceptedConnectionsBody();


            return Task.Run(() => messageBody);
        }
    }
}
