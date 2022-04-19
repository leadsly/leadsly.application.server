using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class ProspectListCommand : ProspectListBaseCommand, ICommand
    {
        public ProspectListCommand(
            ILogger<ProspectListCommand> logger, 
            IMessageBrokerOutlet messageBrokerOutlet, 
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider,
            string prospectListPhaseId, 
            string userId)
            : base(logger, campaignRepositoryFacade, halRepository, timestampService, rabbitMQProvider)
        {
            _prospectListPhaseId = prospectListPhaseId;
            _userId = userId;
            _logger = logger;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly string _prospectListPhaseId;
        private readonly string _userId;
        private readonly ILogger<ProspectListCommand> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// Triggered upon the creation of new Campaign. If campaign created also needs to create new ProspectList, this phase is executed first. This will then create
        /// PrimaryProspectList, PrimaryProspects, and CampaignProspects.
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            await InternalExecuteAsync();
        }

        private async Task InternalExecuteAsync()
        {
            ProspectListBody messageBody = await CreateMessageBodyAsync();

            string queueNameIn = RabbitMQConstants.NetworkingConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.NetworkingConnections.RoutingKey;
            string halId = messageBody.HalId;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.NetworkingConnections.NetworkingType, RabbitMQConstants.NetworkingConnections.ProspectList);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }

        private async Task<ProspectListBody> CreateMessageBodyAsync()
        {
            ProspectListBody messageBody = await CreateProspectListBodyAsync(_prospectListPhaseId, _userId);

            return messageBody;
        }
    }
}
