﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
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
    public class UncontactedFollowUpMessageCommand : ICommand
    {
        public UncontactedFollowUpMessageCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// Triggered every morning and is meant to go out to those prospects who have accepted our invite, who have NOT replied and have NOT gotten a follow up message.
        /// Scenario: On Monday at 6:45 PM, we get a new connection that accepts our request. The follow up message is set up to go out 5 hours after the connection is created.
        /// Hal work day ends at 7:00 PM on Monday. This means that Monday 6:45 PM plus 5 hours = 11:45 PM. The message falls outside of Hal's work day and will not be triggered Monday.
        /// Tuesday morning, this phase is triggered and it checks for all campaign prospects that are part of active campaign that have accepted our invite (our prospect has),
        /// who have NOT received a follow up message (our prospect has not) and who have not replied (naturally our prospect has not replied since they did not have any messages to respond to).
        /// We then check to see if the follow up message delay (5 hours) plus connection accepted date time (Monday 6:45 PM) falls BEFORE Tuesday 7:00AM. If it does we fire the follow up message
        /// right away. If it falls sometime during the work day, we schedule it for that time, if it falls after the work day we do nothing and let the next recurring job take care of it.
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            // first grab all active campaigns
            using (var scope = _serviceProvider.CreateScope())
            {
                ICampaignRepositoryFacade campaignRepositoryFacade = scope.ServiceProvider.GetRequiredService<ICampaignRepositoryFacade>();
                ICampaignProvider campaignProvider = scope.ServiceProvider.GetRequiredService<ICampaignProvider>();
                IList<Campaign> campaigns = await campaignRepositoryFacade.GetAllActiveCampaignsAsync();
                foreach (Campaign activeCampaign in campaigns)
                {
                    // grab all campaign prospects for each campaign
                    IList<CampaignProspect> campaignProspects = await campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(activeCampaign.CampaignId);
                    // filter the list down to only those campaign prospects who have not yet been contacted
                    List<CampaignProspect> uncontactedProspects = campaignProspects.Where(p => p.Accepted == true && p.Replied == false && p.FollowUpMessageSent == false).ToList();

                    await campaignProvider.SendFollowUpMessagesAsync(uncontactedProspects);
                }
            }
        }
    }
}
