using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Requests;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<bool> ProcessPotentialProspectsRepliesAsync(string halId, NewMessagesRequest request, CancellationToken ct = default)
        {
            bool succeeded = true;
            IList<CampaignProspect> campaignProspectsToUpdate = new List<CampaignProspect>();
            List<CampaignProspect> campaignProspects = await GetActiveCampaignProspectsByHalIdAsync(halId, ct) as List<CampaignProspect>;
            foreach (NewMessage newMessage in request.Items)
            {
                // check if this hal has any prospects in active campaigns that match this prospect                
                if (campaignProspects != null && campaignProspects.Count > 0)
                {
                    try
                    {
                        CampaignProspect campaignProspectToUpdate = campaignProspects.SingleOrDefault(p => p.Name == newMessage.ProspectName);
                        if (campaignProspectToUpdate != null)
                        {
                            campaignProspectToUpdate.Replied = true;
                            campaignProspectToUpdate.FollowUpComplete = true;
                            campaignProspectToUpdate.ResponseMessage = newMessage.ResponseMessage;

                            campaignProspectsToUpdate.Add(campaignProspectToUpdate);

                            await DeleteAnyScheduledFollowUpMessagesAsync(campaignProspectToUpdate.CampaignProspectId, ct);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogWarning("More than one prospect found. We need profile url to be able to figure out which prospect to update");
                    }
                }
            }

            campaignProspectsToUpdate = await _campaignRepositoryFacade.UpdateAllCampaignProspectsAsync(campaignProspectsToUpdate, ct);
            if (campaignProspectsToUpdate == null)
            {
                succeeded = false;
            }
            else
            {
                succeeded = true;
            }

            return succeeded;
        }

        public async Task<IList<CampaignProspect>> GetActiveCampaignProspectsByHalIdAsync(string halId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(halId, out IList<CampaignProspect> campaignProspects) == false)
            {
                campaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId, ct);
                _memoryCache.Set(halId, campaignProspects, TimeSpan.FromMinutes(3));
            }

            return campaignProspects;
        }

        public async Task DeleteAnyScheduledFollowUpMessagesAsync(string campaignProspectId, CancellationToken ct = default)
        {
            // check if this user has any scheduled follow up messages that still have to go out
            List<FollowUpMessageJob> followUpMessageJobs = await _followUpMessageJobRepository.GetAllByCampaignProspectIdAsync(campaignProspectId, ct) as List<FollowUpMessageJob>;
            followUpMessageJobs.ForEach(followUpMessageJob =>
            {
                _logger.LogDebug($"Removing hangfire job with id {followUpMessageJob.HangfireJobId}");
                _hangfireService.Delete(followUpMessageJob.HangfireJobId);
            });
        }
    }
}
