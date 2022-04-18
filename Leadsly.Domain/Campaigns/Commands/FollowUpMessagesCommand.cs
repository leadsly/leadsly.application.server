using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class FollowUpMessagesCommand : ICommand
    {
        public FollowUpMessagesCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider, string halId, string userId)
        {
            _halId = halId;
            _userId = userId;
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;
        }

        private readonly string _halId;
        private readonly string _userId;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// FollowUpMessagePhase triggered after DeepScanProspectsForRepliesPhase is finished running. This phase is triggered by hal once
        /// DeepScanProspectsForRepliesPhase has completed running, which runs every morning. Runs FollowUpMessagePhase on all eligible campaign prospects for the given 
        /// Hal id that meet the following conditions. CampaignProspect accepted connection request, has not replied and has gotten a follow up message.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();                
                ISendFollowUpMessageService sendFollowUpMessageService = scope.ServiceProvider.GetRequiredService<ISendFollowUpMessageService>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IList<Campaign> campaigns = await campaignRepositoryFacade.GetAllActiveCampaignsByHalIdAsync(_halId);

                IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();
                foreach (Campaign activeCampaign in campaigns)
                {
                    // grab all campaign prospects for each campaign
                    IList<CampaignProspect> campaignProspects = await campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                    // filter the list down to only those campaign prospects who have not yet been contacted
                    List<CampaignProspect> uncontactedProspects = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpMessageSent == true).ToList();

                    // await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                    messagesGoingOut.Concat(await sendFollowUpMessageService.SendFollowUpMessagesAsync(uncontactedProspects));
                }

                await PublishMessagesGoingOut(messagesGoingOut, rabbitMQProvider);
            }
        }

        private async Task PublishMessagesGoingOut(IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset> messagesGoingOut, IRabbitMQProvider rabbitMQProvider)
        {
            foreach (var item in messagesGoingOut)
            {
                FollowUpMessageBody followUpMessageBody = await rabbitMQProvider.CreateFollowUpMessageBodyAsync(item.Key.CampaignProspectFollowUpMessageId, item.Key.CampaignProspect.CampaignId);
                string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
                string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
                string halId = followUpMessageBody.HalId;

                if (item.Value == default)
                {
                    _messageBrokerOutlet.PublishPhase(followUpMessageBody, queueNameIn, routingKeyIn, halId);
                }
                else
                {
                    BackgroundJob.Schedule<IMessageBrokerOutlet>(x => x.PublishPhase(followUpMessageBody, queueNameIn, routingKeyIn, halId, null), item.Value);
                }
            }
        }
    }
}
