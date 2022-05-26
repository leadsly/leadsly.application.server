using Domain.Models.Responses.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Converters;
using Microsoft.AspNetCore.JsonPatch;
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
        public async Task<HalOperationResult<T>> ProcessProspectsAsync<T>(CollectedProspectsRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // only process those prospects who have not yet been added to the prospect list
            CampaignProspectList campaignProspects = await _campaignRepositoryFacade.GetCampaignProspectListByListIdAsync(request.CampaignProspectListId, ct);
            IList<PrimaryProspectRequest> prospectsToProcess = request.Prospects.Where(x => !campaignProspects.CampaignProspects.Any(p => x.ProfileUrl == p.ProfileUrl)).ToList();
            string campaignId = request.CampaignId;
            string campaignProspectListId = request.CampaignProspectListId;

            if(prospectsToProcess.Count > 0)
            {
                result = await _campaignPhaseProcessorProvider.ProcessProspectsAsync<T>(prospectsToProcess, campaignId, campaignProspectListId, ct);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }            

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> UpdateProspectListPhaseAsync<T>(string prospectListPhaseId, JsonPatchDocument<ProspectListPhase> patchDoc, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            ProspectListPhase phaseToUpdate = await _campaignRepositoryFacade.GetProspectListPhaseByIdAsync(prospectListPhaseId, ct);

            patchDoc.ApplyTo(phaseToUpdate);
            phaseToUpdate = await _campaignRepositoryFacade.UpdateProspectListPhaseAsync(phaseToUpdate, ct);
            if(phaseToUpdate == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResultViewModel<T>> PatchUpdateSocialAccountAsync<T>(string socialAccountId, JsonPatchDocument<SocialAccount> patchDoc, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            SocialAccount socialAccountToUpdate = await _socialAccountRepository.GetByIdAsync(socialAccountId, ct);
            if(socialAccountToUpdate == null)
            {
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.ERROR,
                    Detail = $"Failed to locate social account by id {socialAccountId}",
                    Reason = "The provided social account id must be wrong or the social account user does not exist in the database"
                });
                return result;
            }

            patchDoc.ApplyTo(socialAccountToUpdate);
            socialAccountToUpdate = await _socialAccountRepository.UpdateAsync(socialAccountToUpdate, ct);
            if(socialAccountToUpdate == null)
            {
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.ERROR,
                    Detail = $"Failed to update social account with id {socialAccountId}",
                    Reason = "Error occured when trying to update social account"
                });
                return result;
            }

            result.OperationResults.Succeeded = true;
            return result;
        }

        public async Task TriggerSendConnectionsPhaseAsync(TriggerSendConnectionsRequest request, CancellationToken ct = default)
        {
            await _campaignPhaseClient.ProduceSendConnectionsPhaseAsync(request.CampaignId, request.UserId, ct);
        }

        /// <summary>
        /// Triggered by hal after DeepScanProspectsForRepliesPhase finishes running.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task TriggerScanProspectsForRepliesPhaseAsync(TriggerScanProspectsForRepliesRequest request, CancellationToken ct = default)
        {
            await _campaignPhaseClient.ProduceScanProspectsForRepliesPhaseAsync(request.HalId, request.UserId, ct);
        }

        /// <summary>
        /// Triggered by hal after DeepScanProspectsForRepliesPhase finishes running.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task TriggerFollowUpMessagesPhaseAsync(TriggerFollowUpMessageRequest request, CancellationToken ct = default)            
        {
            await _campaignPhaseClient.ProduceFollowUpMessagesPhaseAsync(request.HalId, request.UserId, ct);
        }

        public async Task<HalOperationResult<T>> ProcessNewlyAcceptedProspectsAsync<T>(NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            return await _campaignPhaseProcessorProvider.ProcessNewlyAcceptedProspectsAsync<T>(request, ct);
        }

        public async Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(CampaignProspectListRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            return await _campaignPhaseProcessorProvider.ProcessConnectionRequestSentForCampaignProspectsAsync<T>(request, ct);
        }

        public async Task<HalOperationResult<T>> ProcessFollowUpMessageSentAsync<T>(FollowUpMessageSentRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            return await _campaignPhaseProcessorProvider.ProcessFollowUpMessageSentAsync<T>(request, ct);
        }

        public async Task<HalOperationResult<T>> GetSearchUrlProgressAsync<T>(string campaignId, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<Application.Model.Entities.Campaigns.SearchUrlProgress> searchUrlsProgressEntity = await _searchUrlProgressRepository.GetAllByCampaignIdAsync(campaignId, ct);

            IList<Application.Model.Campaigns.SearchUrlProgress> searchUrlsProgres = searchUrlsProgressEntity.Select(s =>
            {
                return new Application.Model.Campaigns.SearchUrlProgress
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

            ISearchUrlProgressResponse searchUrlProgressResponse = new SearchUrlProgressResponse
            {
                SearchUrlsProgress = searchUrlsProgres
            };

            result.Value = (T)searchUrlProgressResponse;
            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> UpdateSearchUrlProgressAsync<T>(string searchUrlProgressId, JsonPatchDocument<Application.Model.Entities.Campaigns.SearchUrlProgress> patchDoc, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            Application.Model.Entities.Campaigns.SearchUrlProgress searchUrlProgressToUpdate = await _searchUrlProgressRepository.GetByIdAsync(searchUrlProgressId, ct);

            patchDoc.ApplyTo(searchUrlProgressToUpdate);
            searchUrlProgressToUpdate = await _searchUrlProgressRepository.UpdateAsync(searchUrlProgressToUpdate, ct);
            if (searchUrlProgressToUpdate == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

    }
}
