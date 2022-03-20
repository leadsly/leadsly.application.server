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

namespace Leadsly.Domain
{
    public class CampaignManager : ICampaignManager
    {
        public CampaignManager(ILogger<CampaignManager> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<CampaignManager> _logger;

        public void ProcessAllActiveCampaigns()
        {
            TriggerProspectListsPhase();

            TriggerConstantCampaignPhaseMessages();

            TriggerConnectionWithdrawPhase();
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

    }
}
