using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Networking;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<GetSearchUrlsProgressResponse> GetSearchUrlProgressAsync(string campaignId, CancellationToken ct = default)
        {
            IList<SearchUrlProgress> searchUrlsProgressEntity = await _searchUrlProgressRepository.GetAllByCampaignIdAsync(campaignId, ct);
            if (searchUrlsProgressEntity == null)
            {
                _logger.LogError("Failed to retreive SearchUrlProgress collection by campaignId {campaignId}", campaignId);
                return null;
            }

            IList<SearchUrlProgressModel> searchUrlsProgress = searchUrlsProgressEntity
                .Where(s => s.Exhausted == false)
                .Select(s =>
                {
                    return new SearchUrlProgressModel
                    {
                        CampaignId = s.CampaignId,
                        Exhausted = s.Exhausted,
                        LastActivityTimestamp = s.LastActivityTimestamp,
                        LastPage = s.LastPage,
                        LastProcessedProspect = s.LastProcessedProspect,
                        SearchUrl = s.SearchUrl,
                        TotalSearchResults = s.TotalSearchResults,
                        SearchUrlProgressId = s.SearchUrlProgressId,
                        StartedCrawling = s.StartedCrawling,
                        WindowHandleId = s.WindowHandleId
                    };
                }).ToList();

            return new()
            {
                Items = searchUrlsProgress
            };
        }

        public async Task<bool> UpdateSearchUrlProgressAsync(string searchUrlProgressId, JsonPatchDocument<SearchUrlProgress> update, CancellationToken ct = default)
        {
            SearchUrlProgress searchUrlProgress = await _searchUrlProgressRepository.GetByIdAsync(searchUrlProgressId, ct);
            if (searchUrlProgress == null)
            {
                _logger.LogError("Failed to retrieve SearchUrlProgress by its ID {0}", searchUrlProgressId);
                return false;
            }

            update.ApplyTo(searchUrlProgress);
            searchUrlProgress = await _searchUrlProgressRepository.UpdateAsync(searchUrlProgress, ct);
            if (searchUrlProgress == null)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ProcessSentConnectionsAsync(string campaignId, ConnectionsSentRequest request, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(campaignId);
            IList<CampaignProspect> contactedProspects = new List<CampaignProspect>();
            foreach (ConnectionSentModel campaignRequest in request.Items)
            {
                CampaignProspect contactedProspect = campaignProspects.FirstOrDefault(c => c.PrimaryProspect.ProfileUrl == campaignRequest.ProfileUrl);
                if (contactedProspect != null)
                {
                    contactedProspect.ConnectionSent = true;
                    contactedProspect.ConnectionSentTimestamp = campaignRequest.ConnectionSentTimestamp;

                    contactedProspects.Add(contactedProspect);
                }
            }

            contactedProspects = await _campaignRepositoryFacade.UpdateAllCampaignProspectsAsync(contactedProspects, ct);
            if (contactedProspects == null)
            {
                return false;
            }

            return true;
        }
    }
}
