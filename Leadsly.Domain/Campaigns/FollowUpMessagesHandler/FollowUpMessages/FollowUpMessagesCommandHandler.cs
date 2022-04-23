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
            if(command.HalId != null)
            {
                await InternalHandleAsync(command.HalId);
            }

            if(command.HalIds != null)
            {
                await InternalExecuteListAsync(command.HalIds);
            }
        }

        /// <summary>
        /// Triggered by hal that has just finished running its DeepScanProspectsForRepliesPhase. This will loop through all campaign prospects for that hal that need to get
        /// a follow up message sent.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task InternalHandleAsync(string halId)
        {
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(halId);

            IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();
            foreach (Campaign activeCampaign in campaigns)
            {
                // grab all campaign prospects for each campaign
                IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                List<CampaignProspect> prospectsForFollowUpMessage = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpMessageSent == true).ToList();
                if(prospectsForFollowUpMessage.Count > 0)
                {
                    // await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                    IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesOut = await _sendFollowUpMessageService.CreateSendFollowUpMessagesAsync(prospectsForFollowUpMessage);
                    messagesGoingOut.Concat(messagesOut);
                }
            }

            await PublishMessagesGoingOut(messagesGoingOut);
        }
        
        /// <summary>
        /// Only triggered for those hals that did NOT have DeepScanProspectsForRepliesPhase executed. DeepScanProspectsForRepliesPhase
        /// will handle execution of FollowUpMessagesPhase and ScanProspectsForReplies, however
        /// it is possible that campaign prospects for specific hal do not return any results for DeepScanProspectsForRepliesPhase to execute,
        /// in such cases we need to directly trigger FollowUpMessagesPhase and ScanProspectsForRepliesPhase
        /// </summary>
        /// <param name="halIds"></param>
        /// <returns></returns>
        private async Task InternalExecuteListAsync(IList<string> halIds)
        {
            foreach (string halId in halIds)
            {
                await InternalHandleAsync(halId);
            }
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
