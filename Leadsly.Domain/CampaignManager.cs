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
        public CampaignManager(ILogger<CampaignManager> logger, IRabbitMQRepository rabbitMQRepository, ICampaignRepository campaignRepository)
        {
            _logger = logger;
            _rabbitMQRepository = rabbitMQRepository;
            _campaignRepository = campaignRepository;
        }

        private readonly ILogger<CampaignManager> _logger;
        private readonly IRabbitMQRepository _rabbitMQRepository;
        private readonly ICampaignRepository _campaignRepository;

        public async Task ProcessAllActiveCampaignsAsync()
        {
            List<Campaign> activeCampaigns = await _campaignRepository.GetAllActiveAsync();

            RabbitMQOptions options = _rabbitMQRepository.GetRabbitMQConfigOptions();

            string prospectListJob = TriggerProspectLists(activeCampaigns);


        }

        private string TriggerProspectLists(List<Campaign> activeCampaigns)
        {
            List<string> prospectPhaseIds = new();

            return BackgroundJob.Enqueue<ICampaignPhaseProducerFacade>(x => x.PublishProspectListMessagesPhase(prospectPhaseIds));
        }


        
    }
}
