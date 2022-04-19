using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessage
{
    public class FollowUpMessageCommandHandler : FollowUpMessageCommandHandlerBase, ICommandHandler<FollowUpMessageCommand>
    {
        public FollowUpMessageCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<FollowUpMessageCommandHandler> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider            
            ) : base(messageBrokerOutlet, logger, campaignRepositoryFacade, halRepository, rabbitMQProvider)
        {

        }

        /// <summary>
        /// FollowUpMessage triggered by MonitorForNewProspectsPhase. If new prospect accepts our connection invite and the follow up message delay falls during Hal's work day
        /// This method is triggered to send out the message to that specific campaign prospect. The campaign prospect is found by the campaignProspectFollowUpMessageId parameter
        /// </summary>
        /// <param name="campaignProspectFollowUpMessageId"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public async Task HandleAsync(FollowUpMessageCommand command)
        {
            await InternalExecuteAsync(command.CampaignProspectFollowUpMessageId, command.CampaignId, command.ScheduleTime);
        }
    }
}
