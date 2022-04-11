using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    partial class Supervisor : ISupervisor
    {

        public async Task<HalOperationResult<T>> ProcessProspectsAsync<T>(ProspectListPhaseCompleteRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<CampaignProspect> campaignProspects = new List<CampaignProspect>();
            IList<PrimaryProspect> prospects = new List<PrimaryProspect>();
            foreach (PrimaryProspectRequest primaryProspectRequest in request.Prospects)
            {
                PrimaryProspect primaryProspect = new()
                {
                    AddedTimestamp = primaryProspectRequest.AddedTimestamp,
                    Name = primaryProspectRequest.Name,
                    Area = primaryProspectRequest.Area,
                    EmploymentInfo = primaryProspectRequest.EmploymentInfo,
                    PrimaryProspectListId = primaryProspectRequest.PrimaryProspectListId,
                    ProfileUrl = primaryProspectRequest.ProfileUrl,
                    SearchResultAvatarUrl = primaryProspectRequest.SearchResultAvatarUrl
                };
                prospects.Add(primaryProspect);

                CampaignProspect campaignProspect = new()
                {
                    PrimaryProspect = primaryProspect,
                    CampaignId = request.CampaignId,
                    ProfileUrl = primaryProspectRequest.ProfileUrl,
                    ConnectionSent = false,
                    ConnectionSentTimestamp = 0,
                    FollowUpMessageSent = false,
                    LastFollowUpMessageSentTimestamp = 0
                };
                campaignProspects.Add(campaignProspect);
            }

            prospects = await _campaignRepository.CreatePrimaryProspectsAsync(prospects, ct);
            if (prospects == null)
            {
                return result;
            }

            campaignProspects = await _campaignRepository.CreateCampaignProspectsAsync(campaignProspects, ct);
            if(campaignProspects == null)
            {
                return result;
            }

            ProspectListPhase campaignProspectListPhase = await _campaignRepository.GetProspectListPhaseByIdAsync(request.CampaignId, ct);
            if (campaignProspectListPhase == null)
            {
                return result;
            }

            campaignProspectListPhase.Completed = true;
            // mark campaign's prospect list phase as completed
            campaignProspectListPhase = await _campaignRepository.UpdateCampaignProspectListPhaseAsync(campaignProspectListPhase, ct);
            if (campaignProspectListPhase == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> TriggerSendConnectionsPhase<T>(TriggerSendConnectionsRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            _campaignManager.TriggerSendConnectionsPhase(request.CampaignId, request.UserId);

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessNewlyAcceptedProspectsAsync<T>(NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<Campaign> usersActiveCampaigns = await _campaignRepository.GetAllActiveByUserIdAsync(request.ApplicationUserId, ct);

            HashSet<string> campaignProspectListIds = new();
            List<CampaignProspect> activeCampaignProspects = new List<CampaignProspect>();
            foreach (Campaign campaign in usersActiveCampaigns)
            {
                // for each active campaign grab the corresponding primary prospect list id
                string campaignProspectListId = campaign.CampaignProspectList.CampaignProspectListId;

                // if the dictionary contains an entry for this primary prospect list just move on
                if (campaignProspectListIds.Contains(campaignProspectListId))
                    continue;

                // get the primary prospect list by its id
                CampaignProspectList campaignProspectList = await _campaignRepository.GetCampaignProspectListByCampaignProspectListIdAsync(campaignProspectListId);
                
                if(campaignProspectList != null)
                {
                    activeCampaignProspects.AddRange(campaignProspectList.CampaignProspects);

                    // try and add that primary prospect list to the dictionary
                    campaignProspectListIds.Add(campaignProspectListId);
                }                
            }

            IList<CampaignProspect> updatedCampaignProspects = new List<CampaignProspect>();
            foreach (NewProspectConnectionRequest newProspectConnectionRequest in request.NewAcceptedProspectsConnections)
            {
                // is the newly connected prospect part of any of the user's campaigns?
                CampaignProspect campaignProspect = activeCampaignProspects.FirstOrDefault(p => p.ProfileUrl.Contains(newProspectConnectionRequest.ProfileUrl));
                if (campaignProspect != null)
                {
                    campaignProspect.Accepted = true;
                    campaignProspect.AcceptedTimestamp = newProspectConnectionRequest.AcceptedTimestamp;
                    updatedCampaignProspects.Add(campaignProspect);
                }
            }

            if(updatedCampaignProspects.Count > 0)
            {
                await _campaignRepository.UpdateCampaignProspectsAsync(updatedCampaignProspects, ct);
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(CampaignProspectListRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<CampaignProspect> campaignProspects = await _campaignRepository.GetCampaignProspectsByIdAsync(request.CampaignId);
            IList<CampaignProspect> contactedProspects = new List<CampaignProspect>();
            foreach (CampaignProspectRequest campaignRequest in request.CampaignProspects)
            {
                CampaignProspect contactedProspect = campaignProspects.FirstOrDefault(c => c.PrimaryProspect.ProfileUrl == campaignRequest.ProfileUrl);
                if(contactedProspect != null)
                {
                    contactedProspect.ConnectionSent = true;
                    contactedProspect.ConnectionSentTimestamp = campaignRequest.ConnectionSentTimestamp;

                    contactedProspects.Add(contactedProspect);
                }                
            }

            contactedProspects = await _campaignRepository.UpdateCampaignProspectsAsync(contactedProspects, ct);
            if(contactedProspects == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }        

    }
}
