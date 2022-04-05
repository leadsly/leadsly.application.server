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

        public async Task<HalOperationResult<T>> ProcessNewMyNetworkConnectionsAsync<T>(MyNetworkNewConnectionsRequest request, CancellationToken ct = default)
        where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // grab each new connection and determine which campaign it came from            

            // to do this we have to get all active campaigns for the given hal machine

            // then check each campaigns prospect list and see if this individual exists there

            // if user exists there grab the first follow up message

            // parse replace any tokens (name)

            // send send the follow up message message to hal to 
            // result = await _campaignPhaseFacade.ProcessNewNetworkConnectionsAsync<T>(request.NewConnectionProspects, request.HalId, ct);

            if (result.Succeeded == false)
            {
                return result;
            }

            return result;
            // if succeeded queue up follow up messages
        }
    }
}
