using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.MonitorForNewConnections;
using Leadsly.Domain.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task ProcessNewlyAcceptedProspectsAsync(string halId, RecentlyAddedProspectsRequest request, CancellationToken ct = default)
        {
            foreach (RecentlyAddedProspectModel recentlyAddedProspect in request.Items)
            {
                string searchProfileUrl = recentlyAddedProspect.ProfileUrl.TrimEnd('/');

                CampaignProspect prospect = await _campaignRepositoryFacade.GetCampaignProspectByProfileUrlAsync(searchProfileUrl, halId, request.ApplicationUserId, ct);
                if (prospect != null)
                {
                    prospect.Accepted = true;
                    prospect.AcceptedTimestamp = recentlyAddedProspect.AcceptedRequestTimestamp;

                    prospect = await _campaignRepositoryFacade.UpdateCampaignProspectAsync(prospect, ct);
                    if (prospect == null)
                    {
                        _logger.LogError("Failed to update CampaignProspect {campaignProspect}. Updating was responsible for updating Accepted property to true", prospect.CampaignProspectId);
                    }
                }
            }

            _logger.LogInformation("Publishing FollowUpMessages for halId {halId}", halId);
            await _followUpMessagesProvider.PublishMessageAsync(halId, ct);
        }
    }
}
