﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CampaignPhaseProcessorProvider : ICampaignPhaseProcessorProvider
    {
        public CampaignPhaseProcessorProvider(
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ILogger<CampaignPhaseProcessorProvider> logger,
            ISendFollowUpMessageProvider sendFollowUpMessageProvider,
            ICampaignPhaseClient campaignPhaseClient)
        {
            _logger = logger;
            _campaignPhaseClient = campaignPhaseClient;
            _sendFollowUpMessageProvider = sendFollowUpMessageProvider;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly ILogger<CampaignPhaseProcessorProvider> _logger;
        private readonly ICampaignPhaseClient _campaignPhaseClient;
        private readonly ISendFollowUpMessageProvider _sendFollowUpMessageProvider;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;

        public async Task<HalOperationResult<T>> ProcessProspectsAsync<T>(IList<PrimaryProspectRequest> prospectsToProcess, string campaignId, string campaignProspectListId, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<CampaignProspect> campaignProspects = new List<CampaignProspect>();
            IList<PrimaryProspect> prospects = new List<PrimaryProspect>();
            foreach (PrimaryProspectRequest primaryProspectRequest in prospectsToProcess)
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
                    CampaignId = campaignId,
                    CampaignProspectListId = campaignProspectListId,
                    Name = primaryProspect.Name,
                    ProfileUrl = primaryProspectRequest.ProfileUrl,
                    ConnectionSent = false,
                    ConnectionSentTimestamp = 0,
                    FollowUpMessageSent = false,
                    LastFollowUpMessageSentTimestamp = 0
                };
                campaignProspects.Add(campaignProspect);
            }

            prospects = await _campaignRepositoryFacade.CreateAllPrimaryProspectsAsync(prospects, ct);
            if (prospects == null)
            {
                return result;
            }

            campaignProspects = await _campaignRepositoryFacade.CreateAllCampaignProspectsAsync(campaignProspects, ct);
            if (campaignProspects == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessNewlyAcceptedProspectsAsync<T>(NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<Campaign> usersActiveCampaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsByUserIdAsync(request.ApplicationUserId, ct);
            string userId = request.ApplicationUserId;
            int activeCampaignsCount = usersActiveCampaigns.Count;
            _logger.LogDebug("User with id {userId}, has {activeCampaignsCount} active campaign(s)", userId, activeCampaignsCount);

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
                IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(campaign.CampaignId, ct);

                if (campaignProspects != null || campaignProspects.Count > 0)
                {
                    activeCampaignProspects.AddRange(campaignProspects);

                    // try and add that primary prospect list to the dictionary
                    campaignProspectListIds.Add(campaign.CampaignProspectList.CampaignProspectListId);
                }
            }

            IList<CampaignProspect> updatedCampaignProspects = new List<CampaignProspect>();
            foreach (NewProspectConnectionRequest newProspectConnectionRequest in request.NewAcceptedProspectsConnections)
            {
                // is the newly connected prospect part of any of the user's campaigns?
                // remove any trailing slashes
                string searchProfileUrl = newProspectConnectionRequest.ProfileUrl.TrimEnd('/');
                CampaignProspect campaignProspect = activeCampaignProspects.FirstOrDefault(p => p.ProfileUrl == searchProfileUrl);
                if (campaignProspect != null)
                {
                    campaignProspect.Accepted = true;
                    campaignProspect.AcceptedTimestamp = newProspectConnectionRequest.AcceptedTimestamp;
                    updatedCampaignProspects.Add(campaignProspect);
                }
            }

            if (updatedCampaignProspects.Count > 0)
            {
                await _campaignRepositoryFacade.UpdateAllCampaignProspectsAsync(updatedCampaignProspects, ct);

                var messagesToGoingOut = await _sendFollowUpMessageProvider.CreateSendFollowUpMessagesAsync(updatedCampaignProspects, ct);
                await _campaignPhaseClient.ProduceSendFollowUpMessagesAsync(messagesToGoingOut, ct);
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessFollowUpMessageSentAsync<T>(FollowUpMessageSentRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            CampaignProspect campaignProspect = await _campaignRepositoryFacade.GetCampaignProspectByIdAsync(request.CampaignProspectId, ct);

            if (campaignProspect.FollowUpMessageSent == false)
            {
                campaignProspect.FollowUpMessageSent = true;
            }

            campaignProspect.LastFollowUpMessageSentTimestamp = request.MessageSentTimestamp;
            campaignProspect.SentFollowUpMessageOrderNum = request.MessageOrderNum;

            campaignProspect = await _campaignRepositoryFacade.UpdateCampaignProspectAsync(campaignProspect, ct);
            if (campaignProspect == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(CampaignProspectListRequest request, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(request.CampaignId);
            IList<CampaignProspect> contactedProspects = new List<CampaignProspect>();
            foreach (CampaignProspectRequest campaignRequest in request.CampaignProspects)
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
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
