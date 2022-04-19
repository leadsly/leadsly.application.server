using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class MonitorForNewConnectionsCommand : ICommand
    {
        public MonitorForNewConnectionsCommand(IMessageBrokerOutlet messageBrokerOutlet, string halId, string userId)
        {
            _halId = halId;
            _userId = userId;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly string _halId;
        private readonly string _userId;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
                
        public async Task ExecuteAsync()
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
