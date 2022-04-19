using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class FollowUpMessageCommand : FollowUpMessageBaseCommand, ICommand
    {        
        public FollowUpMessageCommand(IMessageBrokerOutlet messageBrokerOutlet,  
            ILogger<FollowUpMessageCommand> logger, 
            ICampaignRepositoryFacade campaignRepositoryFacade, 
            IHalRepository halRepository, 
            IRabbitMQProvider rabbitMQProvider, 
            string campaignProspectFollowUpMessageId, 
            string campaignId, 
            DateTimeOffset scheduleTime)
            : base(messageBrokerOutlet, logger, campaignRepositoryFacade, halRepository, rabbitMQProvider)
        {
            _campaignProspectFollowUpMessageId = campaignProspectFollowUpMessageId;
            _campaignId = campaignId;
            _scheduleTime = scheduleTime;
        }

        private readonly string _campaignId;
        private readonly DateTimeOffset _scheduleTime;
        private readonly string _campaignProspectFollowUpMessageId;

        /// <summary>
        /// FollowUpMessage triggered by MonitorForNewProspectsPhase. If new prospect accepts our connection invite and the follow up message delay falls during Hal's work day
        /// This method is triggered to send out the message to that specific campaign prospect. The campaign prospect is found by the campaignProspectFollowUpMessageId parameter
        /// </summary>
        /// <param name="campaignProspectFollowUpMessageId"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            await InternalExecuteAsync(_campaignProspectFollowUpMessageId, _campaignId, _scheduleTime);
        }
    }
}
