using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class PreventFollowUpMessageService : IPreventFollowUpMessageService
    {
        public PreventFollowUpMessageService(ILogger<PreventFollowUpMessageService> logger, ICampaignRepositoryFacade facade, ITimestampService timestampService)
        {
            _timestampService = timestampService;
            _facade = facade;
            _logger = logger;
        }

        private readonly ITimestampService _timestampService;
        private readonly ICampaignRepositoryFacade _facade;
        private readonly ILogger<PreventFollowUpMessageService> _logger;
        public async Task MarkProspectsAsCompleteAsync(string halId)
        {
            _logger.LogInformation("Checking if there are any prospects who have not responded to the last follow up message in over 14 days for HalId {halId}", halId);
            // sample code is below: 
            IList<CampaignProspect> followUpCompleteProspects = new List<CampaignProspect>();
            IList<CampaignProspect> activeCampaignProspects = await _facade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
            DateTimeOffset localNow = await _timestampService.GetNowLocalizedAsync(halId);
            foreach (CampaignProspect prospect in activeCampaignProspects)
            {
                DateTimeOffset lastFollowUpMessageDate = await _timestampService.GetDateFromTimestampLocalizedAsync(halId, prospect.LastFollowUpMessageSentTimestamp);
                if (((prospect.FollowUpMessages.Count - 1) == prospect.SentFollowUpMessageOrderNum) && ((lastFollowUpMessageDate - localNow).TotalDays < 14))
                {
                    _logger.LogDebug("Prospect {0} with campaign prospect ID {1} has not responded to our last follow up message in over 14 days. Marking them as complete", prospect.Name, prospect.CampaignProspectId);
                    prospect.FollowUpComplete = true;
                    prospect.FollowUpCompleteTimestamp = _timestampService.CreateNowTimestamp();

                    followUpCompleteProspects.Add(prospect);
                }
            };

            await _facade.UpdateAllCampaignProspectsAsync(followUpCompleteProspects);
        }
    }
}
