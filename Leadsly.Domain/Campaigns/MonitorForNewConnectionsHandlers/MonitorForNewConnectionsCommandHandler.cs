using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsCommandHandler : MonitorForNewConnectionsCommandHandlerBase, ICommandHandler<MonitorForNewConnectionsCommand>
    {
        public MonitorForNewConnectionsCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<MonitorForNewConnectionsCommandHandler> logger,
            IUserProvider userProvider,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider
            ) : base(userProvider, campaignRepositoryFacade, rabbitMQProvider, halRepository, timestampService, logger)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsCommandHandler> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        public async Task HandleAsync(MonitorForNewConnectionsCommand command)
        {
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            string halId = command.HalId;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase);

            MonitorForNewAcceptedConnectionsBody messageBody = await CreateMessageBodyAsync();
            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
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
