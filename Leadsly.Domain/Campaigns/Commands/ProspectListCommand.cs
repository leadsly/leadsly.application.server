using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
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
    public class ProspectListCommand : ICommand
    {
        public ProspectListCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider, string prospectListPhaseId, string userId)
        {
            _prospectListPhaseId = prospectListPhaseId;
            _userId = userId;
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;
        }

        private readonly string _prospectListPhaseId;
        private readonly string _userId;
        private readonly IServiceProvider _serviceProvider;
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
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                ProspectListBody messageBody = await rabbitMQProvider.CreateProspectListBodyAsync(_prospectListPhaseId, _userId);

                return messageBody;
            }
        }
    }
}
