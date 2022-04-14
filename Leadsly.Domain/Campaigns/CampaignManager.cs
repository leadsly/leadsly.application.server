using Hangfire;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns
{
    public class CampaignManager : ICampaignManager
    {
        public CampaignManager(ILogger<CampaignManager> logger, ICampaignPhaseProducer campaignPhaseProducer)
        {
            _logger = logger;
            _campaignPhaseProducer = campaignPhaseProducer;
        }

        private readonly ICampaignPhaseProducer _campaignPhaseProducer;
        private readonly ILogger<CampaignManager> _logger;

        public async Task ProcessAllActiveCampaignsAsync()
        {
            //TriggerProspectListsPhase();

            //TriggerConstantCampaignPhaseMessages();

            //TriggerConnectionWithdrawPhase();

            // TriggerMonitorForNewConnectionsPhase();

            await TriggerFollowUpMessagePhaseForUncontactedProspectsAsync();

            await TriggerScanProspectsForRepliesFromOffHoursAsync();
        }
        
        private static void TriggerScanProspectsForRepliesFromOffHours()
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishScanProspectsForRepliesFromOffHoursAsync());
        }
        private async Task TriggerScanProspectsForRepliesFromOffHoursAsync()
        {
            await _campaignPhaseProducer.PublishScanProspectsForRepliesFromOffHoursAsync();
        }

        private static void TriggerFollowUpMessagePhaseForUncontactedProspects()
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishFollowUpMessagePhaseMessagesAsync());
        }

        private async Task TriggerFollowUpMessagePhaseForUncontactedProspectsAsync()
        {
            await _campaignPhaseProducer.PublishFollowUpMessagePhaseMessagesAsync();
        }

        private static void TriggerMonitorForNewConnectionsPhase()
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishMonitorForNewConnectionsPhaseMessageAsync());
        }

        private static void TriggerConstantCampaignPhaseMessages()
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishConstantCampaignPhaseMessagesAsync());
        }

        private static void TriggerProspectListsPhase()
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishProspectListPhaseMessagesAsync());
        }

        private static void TriggerConnectionWithdrawPhase()
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishConnectionWithdrawPhaseMessages());
        }

        public void TriggerProspectListPhase(string prospectListPhaseId, string userId)
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishProspectListPhaseMessagesAsync(prospectListPhaseId, userId));
        }

        public void TriggerSendConnectionsPhase(string campaignId, string userId)
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishSendConnectionsPhaseMessageAsync(campaignId, userId));
        }

        public async Task TriggerFollowUpMessagePhaseAsync(string campaignProspectFollowUpMessageId, string campaignId, DateTimeOffset scheduleTime = default)
        {
            if(scheduleTime == default)
            {
                await _campaignPhaseProducer.PublishFollowUpMessagePhaseMessageAsync(campaignProspectFollowUpMessageId, campaignId);
            }
            else
            {
                BackgroundJob.Schedule<ICampaignPhaseProducer>(x => x.PublishFollowUpMessagePhaseMessageAsync(campaignProspectFollowUpMessageId, campaignId), scheduleTime);
            }
        }
    }
}
