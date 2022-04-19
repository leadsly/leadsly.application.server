using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
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
    public class ProspectListsCommand : ProspectListBaseCommand, ICommand
    {
        public ProspectListsCommand
            (ILogger<ProspectListsCommand> logger, 
            IMessageBrokerOutlet messageBrokerOutlet, 
            ICampaignProvider campaignProvider,            
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            ITimestampService timestampService,
            IRabbitMQProvider rabbitMQProvider
            )
            : base(logger, campaignRepositoryFacade, halRepository, timestampService, rabbitMQProvider)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _campaignProvider = campaignProvider;            
        }

        private readonly ICampaignProvider _campaignProvider;
        private readonly ILogger<ProspectListsCommand> _logger;
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
            IList<ProspectListPhase> prospectListPhases = await CreateMessageBodiesAsync();

            // if there are values
            if (prospectListPhases.Any() == true)
            {
                // iterate over each hal id and fire off the message
                foreach (ProspectListPhase prospectListPhase in prospectListPhases)
                {
                    ProspectListBody messageBody = await CreateProspectListBodyAsync(prospectListPhase.ProspectListPhaseId, prospectListPhase.Campaign.ApplicationUserId);

                    InternalExecute(messageBody);
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

        private async Task<IList<ProspectListPhase>> CreateMessageBodiesAsync()
        {
            IList<ProspectListPhase> prospectListPhases = await _campaignProvider.GetIncompleteProspectListPhasesAsync();

            return prospectListPhases;
        }
    }
}
