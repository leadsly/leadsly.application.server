using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class ProspectListsCommand : ICommand
    {
        public ProspectListsCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;            
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;                

        /// <summary>
        /// Triggered on recurring bases once in the morning. This phase is meant to trigger to create ProspectLists for campaigns that were created outside of Hal work hours.
        /// This means if user created campaign at 10:00PM local time, the campaign ProspectListPhase will not be triggered until the following morning.
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            await InternalExecuteListAsync();
        }
        
        private async Task InternalExecuteListAsync()
        {
            // get the payload
            HalsProspectListPhasesPayload payload = await CreateMessageBodiesAsync();

            // if there are values
            if (payload.ProspectListPayload.Any() == true)
            {
                // iterate over each hal id and fire off the message
                foreach (string id in payload.ProspectListPayload.Keys)
                {
                    string halId = id;
                    List<ProspectListBody> messageBodies = payload.ProspectListPayload[halId];

                    foreach (ProspectListBody messageBody in messageBodies)
                    {
                        InternalExecute(messageBody);
                    }
                }
            }
        }

        private void InternalExecute(ProspectListBody messageBody)
        {
            string queueNameIn = RabbitMQConstants.NetworkingConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.NetworkingConnections.RoutingKey;
            string halId = messageBody.HalId;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.NetworkingConnections.NetworkingType, RabbitMQConstants.NetworkingConnections.ProspectList);

            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, headers);
        }

        private async Task<HalsProspectListPhasesPayload> CreateMessageBodiesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                HalsProspectListPhasesPayload payload = await campaignProvider.GetIncompleteProspectListPhasesAsync();

                return payload;
            }
        }
    }
}
