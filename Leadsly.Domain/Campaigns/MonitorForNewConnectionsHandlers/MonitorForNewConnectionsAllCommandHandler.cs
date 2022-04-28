using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandler;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers;
using Leadsly.Domain.Facades.Interfaces;
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

namespace Leadsly.Domain.Campaigns.Handlers
{
    public class MonitorForNewConnectionsAllCommandHandler : MonitorForNewConnectionsCommandHandlerBase, ICommandHandler<MonitorForNewConnectionsAllCommand>
    {
        public MonitorForNewConnectionsAllCommandHandler(IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<MonitorForNewConnectionsAllCommandHandler> logger,
            IUserProvider userProvider,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider) : base(userProvider, campaignRepositoryFacade, rabbitMQProvider, halRepository, timestampService, logger)
        {           
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsAllCommandHandler> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;        

        public async Task HandleAsync(MonitorForNewConnectionsAllCommand command)
        {
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase);

            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = await CreateMessageBodiesAsync();
            foreach (MonitorForNewAcceptedConnectionsBody body in messageBodies)
            {
                string halId = body.HalId;
                _messageBrokerOutlet.PublishPhase(body, queueNameIn, routingKeyIn, halId, headers);
            }
        }
    }
}
