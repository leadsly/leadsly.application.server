using Leadsly.Application.Model.Entities.Campaigns;
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

namespace Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessages
{
    public class FollowUpMessagesCommandHandler : FollowUpMessageCommandHandlerBase, ICommandHandler<FollowUpMessagesCommand>
    {
        public FollowUpMessagesCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<FollowUpMessagesCommandHandler> logger,
            IHalRepository halRepository,
            IRabbitMQProvider rabbitMQProvider,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ISendFollowUpMessageProvider sendFollowUpMessageService
            ) : base(messageBrokerOutlet, logger, campaignRepositoryFacade, halRepository, rabbitMQProvider)
        {
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _sendFollowUpMessageService = sendFollowUpMessageService;
        }

        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ISendFollowUpMessageProvider _sendFollowUpMessageService;


        /// <summary>
        /// FollowUpMessagePhase triggered after DeepScanProspectsForRepliesPhase is finished running. This phase is triggered by hal once
        /// DeepScanProspectsForRepliesPhase has completed running, which runs every morning. Runs FollowUpMessagePhase on all eligible campaign prospects for the given 
        /// Hal id that meet the following conditions. CampaignProspect accepted connection request, has not replied and has gotten a follow up message.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task HandleAsync(FollowUpMessagesCommand command)
        {
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(command.HalId);

            IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();
            foreach (Campaign activeCampaign in campaigns)
            {
                // grab all campaign prospects for each campaign
                IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                List<CampaignProspect> uncontactedProspects = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpMessageSent == true).ToList();

                // await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesOut = await _sendFollowUpMessageService.CreateSendFollowUpMessagesAsync(uncontactedProspects);
                messagesGoingOut.Concat(messagesOut);
            }

            await PublishMessagesGoingOut(messagesGoingOut);
        }

        private async Task PublishMessagesGoingOut(IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut)
        {
            foreach (var messagePair in messagesGoingOut)
            {
                await InternalExecuteAsync(messagePair.Key.CampaignProspectFollowUpMessageId, messagePair.Key.CampaignProspect.CampaignId, messagePair.Value);
            }
        }
    }
}
