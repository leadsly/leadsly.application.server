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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectList
{
    public class ProspectListCommandHandler : ProspectListCommandHandlerBase, ICommandHandler<ProspectListCommand>
    {
        public ProspectListCommandHandler(
            ILogger<ProspectListCommandHandler> logger,
            IMessageBrokerOutlet messageBrokerOutlet,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,            
            IRabbitMQProvider rabbitMQProvider
            ) : base(logger, campaignRepositoryFacade, halRepository, rabbitMQProvider)
        {
            _logger = logger;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly ILogger<ProspectListCommandHandler> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// Triggered upon the creation of new Campaign. If campaign created also needs to create new ProspectList, this phase is executed first. This will then create
        /// PrimaryProspectList, PrimaryProspects, and CampaignProspects.
        /// </summary>
        /// <returns></returns>
        public async Task HandleAsync(ProspectListCommand command)
        {
            await InternalExecuteAsync(command);
        }

        private async Task InternalExecuteAsync(ProspectListCommand command)
        {
            ProspectListBody messageBody = await CreateMessageBodyAsync(command);

            string queueNameIn = RabbitMQConstants.NetworkingConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.NetworkingConnections.RoutingKey;
            string halId = messageBody.HalId;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.NetworkingConnections.NetworkingType, RabbitMQConstants.NetworkingConnections.ProspectList);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }

        private async Task<ProspectListBody> CreateMessageBodyAsync(ProspectListCommand command)
        {
            ProspectListBody messageBody = await CreateProspectListBodyAsync(command.ProspectListPhaseId, command.UserId);

            return messageBody;
        }
    }
}
