using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
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
    public class FollowUpMessageCommand : ICommand
    {        
        public FollowUpMessageCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider, string campaignProspectFollowUpMessageId, string campaignId)
        {
            _campaignProspectFollowUpMessageId = campaignProspectFollowUpMessageId;
            _campaignId = campaignId;
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;
        }

        private readonly string _campaignId;
        private readonly string _campaignProspectFollowUpMessageId;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// FollowUpMessage triggered by MonitorForNewProspectsPhase. If new prospect accepts our connection invite and the follow up message delay falls during Hal's work day
        /// This method is triggered to send out the message to that specific campaign prospect. The campaign prospect is found by the campaignProspectFollowUpMessageId parameter
        /// </summary>
        /// <param name="campaignProspectFollowUpMessageId"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();

                // grab the CampaignProspectFollowUpMessage
                FollowUpMessageBody followUpMessageBody = await rabbitMQProvider.CreateFollowUpMessageBodyAsync(_campaignProspectFollowUpMessageId, _campaignId);

                string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
                string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
                string halId = followUpMessageBody.HalId;

                _messageBrokerOutlet.PublishPhase(followUpMessageBody, queueNameIn, routingKeyIn, halId);
            }
        }
    }
}
