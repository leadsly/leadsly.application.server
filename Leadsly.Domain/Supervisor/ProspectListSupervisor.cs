using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.ProspectList;
using Leadsly.Domain.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    partial class Supervisor : ISupervisor
    {
        public async Task<bool> ProcessProspectsAsync(CollectedProspectsRequest request, CancellationToken ct = default)
        {
            PrimaryProspectList primaryProspectList = await _campaignRepositoryFacade.GetPrimaryProspectListByIdAsync(request.PrimaryProspectListId, ct);
            if (primaryProspectList.PrimaryProspects == null)
            {
                _logger.LogError("Cannot successfully process primary prospects because PrimaryProspects collection is null");
                return false;
            }

            IList<CampaignProspect> campaignProspects = new List<CampaignProspect>();
            IList<PrimaryProspect> prospects = new List<PrimaryProspect>();
            foreach (PersistPrimaryProspectModel newPrimaryProspect in request.Items)
            {
                // check if newPrimaryProspect doesn't already exist for this primary prospect list id
                if (primaryProspectList.PrimaryProspects.Any(p => p.ProfileUrl == newPrimaryProspect.ProfileUrl && p.Name == p.Name) == false)
                {
                    PrimaryProspect primaryProspect = new()
                    {
                        AddedTimestamp = newPrimaryProspect.AddedTimestamp,
                        Name = newPrimaryProspect.Name,
                        Area = newPrimaryProspect.Area,
                        EmploymentInfo = newPrimaryProspect.EmploymentInfo,
                        PrimaryProspectListId = request.PrimaryProspectListId,
                        ProfileUrl = newPrimaryProspect.ProfileUrl,
                        SearchResultAvatarUrl = newPrimaryProspect.SearchResultAvatarUrl
                    };
                    prospects.Add(primaryProspect);

                    CampaignProspect campaignProspect = new()
                    {
                        PrimaryProspect = primaryProspect,
                        CampaignId = request.CampaignId,
                        CampaignProspectListId = request.CampaignProspectListId,
                        Name = primaryProspect.Name,
                        ProfileUrl = newPrimaryProspect.ProfileUrl,
                        ConnectionSent = false,
                        ConnectionSentTimestamp = 0,
                        FollowUpMessageSent = false,
                        LastFollowUpMessageSentTimestamp = 0
                    };
                    campaignProspects.Add(campaignProspect);
                }
            }

            if (prospects.Count > 0)
            {
                prospects = await _campaignRepositoryFacade.CreateAllPrimaryProspectsAsync(prospects, ct);
            }

            if (campaignProspects.Count > 0)
            {
                campaignProspects = await _campaignRepositoryFacade.CreateAllCampaignProspectsAsync(campaignProspects, ct);
            }

            if (prospects == null || campaignProspects == null)
            {
                _logger.LogError("Failed to successfully process new Primary and Campaign prospects. Failed persisting them to the database");
                return false;
            }

            return true;
        }
    }
}
