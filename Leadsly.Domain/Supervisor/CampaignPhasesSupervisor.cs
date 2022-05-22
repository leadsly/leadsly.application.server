﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
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

        public async Task<HalOperationResult<T>> UpdateProspectListPhaseAsync<T>(string prospectListPhaseId, JsonPatchDocument<ProspectListPhase> patchDoc, CancellationToken ct)
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

    }
}
