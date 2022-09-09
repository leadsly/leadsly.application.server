using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.MonitorForNewConnections;
using Leadsly.Domain.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task ProcessNewlyAcceptedProspectsAsync(string halId, RecentlyAddedProspectsRequest request, CancellationToken ct = default)
        {
            bool anyCampaignProspects = false;
            IList<Campaign> activeCampaigns = new List<Campaign>();
            foreach (RecentlyAddedProspectModel recentlyAddedProspect in request.Items)
            {
                string searchProfileUrl = recentlyAddedProspect.ProfileUrl.TrimEnd('/');

                CampaignProspect prospect = await _campaignRepositoryFacade.GetCampaignProspectByProfileUrlAsync(searchProfileUrl, halId, request.ApplicationUserId, ct);
                if (prospect != null)
                {
                    anyCampaignProspects = true;
                    prospect.Accepted = true;
                    prospect.AcceptedTimestamp = recentlyAddedProspect.AcceptedRequestTimestamp;

                    if (prospect.Campaign != null)
                    {
                        _logger.LogDebug("Campaign prospect {prospectId} has a campaign associated with them", prospect.CampaignProspectId);

                        prospect = await _campaignRepositoryFacade.UpdateCampaignProspectAsync(prospect, ct);
                        if (prospect == null)
                        {
                            _logger.LogError("Failed to update CampaignProspect {campaignProspect}. Updating was responsible for updating Accepted property to true", prospect.CampaignProspectId);
                        }
                        else
                        {
                            activeCampaigns.Add(prospect.Campaign);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Campaign prospect {prospectId} does not have a campaign associated with them", prospect.CampaignProspectId);
                    }
                }
            }

            if (anyCampaignProspects == true)
            {
                _logger.LogInformation("Publishing FollowUpMessages for halId {halId}", halId);
                await _mqCreatorFacade.PublishFollowUpMessagesMessageAsync(halId, activeCampaigns, ct);
            }
        }
    }
}
