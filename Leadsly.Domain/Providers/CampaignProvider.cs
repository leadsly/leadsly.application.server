using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CampaignProvider : ICampaignProvider
    {
        public CampaignProvider(
            ILogger<CampaignProvider> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHangfireService hangfireService,
            IMemoryCache memoryCache,
            IFollowUpMessageJobsRepository followUpMessageJobRepository
            )
        {
            _hangfireService = hangfireService;
            _followUpMessageJobRepository = followUpMessageJobRepository;
            _memoryCache = memoryCache;
            _logger = logger;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IHangfireService _hangfireService;
        private readonly IFollowUpMessageJobsRepository _followUpMessageJobRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CampaignProvider> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;

        /// <summary>
        /// This is used by DeepScanProspectsForRepliesPhase
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<HalOperationResult<T>> ProcessCampaignProspectsRepliedAsync<T>(ProspectsRepliedRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<CampaignProspect> campaignProspectsToUpdate = new List<CampaignProspect>();
            foreach (ProspectRepliedRequest prospectReplied in request.ProspectsReplied)
            {
                CampaignProspect campaignProspectToUpdate = await _campaignRepositoryFacade.GetCampaignProspectByIdAsync(prospectReplied.CampaignProspectId, ct);

                campaignProspectToUpdate.Replied = true;
                campaignProspectToUpdate.ResponseMessage = prospectReplied.ResponseMessage;
                campaignProspectsToUpdate.Add(campaignProspectToUpdate);
            }

            campaignProspectsToUpdate = await _campaignRepositoryFacade.UpdateAllCampaignProspectsAsync(campaignProspectsToUpdate, ct);
            if (campaignProspectsToUpdate == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        /// <summary>
        /// This is used by ScanProspectsForRepliesPhase
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<HalOperationResult<T>> ProcessProspectsRepliedAsync<T>(ProspectsRepliedRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            string halId = request.HalId;
            IList<CampaignProspect> campaignProspectsToUpdate = new List<CampaignProspect>();
            foreach (ProspectRepliedRequest prospectReplied in request.ProspectsReplied)
            {
                // check if this hal has any prospects in active campaigns that match this prospect
                List<CampaignProspect> campaignProspects = await GetActiveCampaignProspectsByHalIdAsync(halId, ct) as List<CampaignProspect>;
                if (campaignProspects != null && campaignProspects.Count > 0)
                {
                    try
                    {
                        CampaignProspect campaignProspectToUpdate = campaignProspects.SingleOrDefault(p => p.Name == prospectReplied.ProspectName);
                        if (campaignProspectToUpdate != null)
                        {
                            campaignProspectToUpdate.Replied = true;
                            campaignProspectToUpdate.FollowUpComplete = true;
                            campaignProspectToUpdate.ResponseMessage = prospectReplied.ResponseMessage;

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
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private async Task DeleteAnyScheduledFollowUpMessagesAsync(string campaignProspectId, CancellationToken ct = default)
        {
            // check if this user has any scheduled follow up messages that still have to go out
            List<FollowUpMessageJob> followUpMessageJobs = await _followUpMessageJobRepository.GetAllByCampaignProspectIdAsync(campaignProspectId, ct) as List<FollowUpMessageJob>;
            followUpMessageJobs.ForEach(followUpMessageJob =>
            {
                _logger.LogDebug($"Removing hangfire job with id {followUpMessageJob.HangfireJobId}");
                _hangfireService.Delete(followUpMessageJob.HangfireJobId);
            });
        }

        private async Task<IList<CampaignProspect>> GetActiveCampaignProspectsByHalIdAsync(string halId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(halId, out IList<CampaignProspect> campaignProspects) == false)
            {
                campaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId, ct);
                _memoryCache.Set(halId, campaignProspects, TimeSpan.FromMinutes(3));
            }

            return campaignProspects;
        }

        /// <summary>
        /// Grabs all ProspectListPhases that have not completed. Each campaign that creates new PropsectList will have
        /// a ProspectListPhase. ProspectListPhase is triggered first to gather all prospects in the given search urls.
        /// If campaign is created after Hal's work hours, the ProspectListPhase is not triggered until the next work day.
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns></returns>
        public async Task<IList<ProspectListPhase>> GetIncompleteProspectListPhasesAsync(string halId, CancellationToken ct = default)
        {
            IList<ProspectListPhase> prospectListPhases = await _campaignRepositoryFacade.GetAllActiveProspectListPhasesAsync(ct);
            IList<ProspectListPhase> incompleteProspectListPhases = prospectListPhases.Where(p => p.Completed == false).ToList();

            return incompleteProspectListPhases;
        }

        public async Task<HalOperationResult<T>> UpdateSentConnectionsUrlStatusesAsync<T>(string campaignId, UpdateSearchUrlDetailsRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<SearchUrlDetails> searchUrlDetails = await _campaignRepositoryFacade.GetAllSentConnectionsStatusesAsync(campaignId, ct);
            foreach (SearchUrlDetailsRequest payload in request.SearchUrlDetailsRequests)
            {
                SearchUrlDetails update = searchUrlDetails.Where(s => s.SearchUrlDetailsId == payload.SearchUrlDetailsId).FirstOrDefault();
                if (update == null)
                {
                    continue;
                }
                update.LastActivityTimestamp = payload.LastActivityTimestamp;
                update.FinishedCrawling = payload.FinishedCrawling;
                update.StartedCrawling = payload.StartedCrawling;
                update.CurrentUrl = payload.CurrentUrl;
                update.WindowHandleId = payload.WindowHandleId;

                update = await _campaignRepositoryFacade.UpdateSentConnectionsStatusAsync(update, ct);
                if (update == null)
                {
                    return result;
                }
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> GetSentConnectionsUrlStatusesAsync<T>(string campaignId, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<SearchUrlDetails> sentConnectionsSearchUrlStatuses = await _campaignRepositoryFacade.GetAllSentConnectionsStatusesAsync(campaignId, ct);
            if (sentConnectionsSearchUrlStatuses == null)
            {
                return result;
            }

            IList<SearchUrlDetailsRequest> sentConnectionsSearchUrlStatusesPayload = sentConnectionsSearchUrlStatuses
                                                                                                .Where(s => s.FinishedCrawling == false)
                                                                                                .Select(s =>
                                                                                                {
                                                                                                    return new SearchUrlDetailsRequest
                                                                                                    {
                                                                                                        SearchUrlDetailsId = s.SearchUrlDetailsId,
                                                                                                        CurrentUrl = s.CurrentUrl,
                                                                                                        LastActivityTimestamp = s.LastActivityTimestamp,
                                                                                                        OriginalUrl = s.OriginalUrl,
                                                                                                        WindowHandleId = s.WindowHandleId,
                                                                                                        StartedCrawling = s.StartedCrawling,
                                                                                                        FinishedCrawling = s.FinishedCrawling
                                                                                                    };
                                                                                                })
                                                                                                .ToList();

            IGetSentConnectionsUrlStatusPayload payload = new GetSentConnectionsUrlStatusPayload
            {
                SearchUrlDetailsRequests = sentConnectionsSearchUrlStatusesPayload
            };

            result.Value = (T)payload;
            result.Succeeded = true;
            return result;
        }

        public async Task<IList<Campaign>> GetAllByUserIdAsync(string userId, CancellationToken ct = default)
        {
            IList<Campaign> campaigns = await _campaignRepositoryFacade.GetAllCampaignsByUserIdAsync(userId, ct);

            return campaigns;
        }

        public async Task<long> GetTotalConnectionsSentAsync(string campaignId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetCampaignProspectsAsync(campaignId, ct);
            IList<CampaignProspect> setnConnectionsProspects = campaignProspects?.Where(p => p.ConnectionSent == true).ToList();

            if (setnConnectionsProspects == null || setnConnectionsProspects.Count == 0)
            {
                _logger.LogInformation("Campaign {campaignId} has no sent connections", campaignId);
                return 0;
            }

            return setnConnectionsProspects.Count;
        }

        public async Task<long> GetConnectionsAcceptedAsync(string campaignId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetCampaignProspectsAsync(campaignId, ct);
            IList<CampaignProspect> connectionsAccepted = campaignProspects?.Where(p => p.Accepted == true).ToList();

            if (connectionsAccepted == null || connectionsAccepted.Count == 0)
            {
                _logger.LogInformation("Campaign {campaignId} has no accepted connections", campaignId);
                return 0;
            }

            return connectionsAccepted.Count;
        }

        public async Task<long> GetRepliesAsync(string campaignId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetCampaignProspectsAsync(campaignId, ct);
            IList<CampaignProspect> connectionReplied = campaignProspects?.Where(p => p.Replied == true).ToList();

            if (connectionReplied == null || connectionReplied.Count == 0)
            {
                _logger.LogInformation("Campaign {campaignId} has no prospects that replied to any of the messages yet.", campaignId);
                return 0;
            }

            return connectionReplied.Count;
        }

        public async Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepositoryFacade.GetCampaignByIdAsync(campaignId, ct);
        }

        public async Task<Campaign> UpdateCampaignAsync(Campaign campaign, CancellationToken ct = default)
        {
            campaign = await _campaignRepositoryFacade.UpdateCampaignAsync(campaign, ct);
            if (campaign == null)
            {
                _logger.LogError("Failed to update campaign {campaignId}", campaign.CampaignId);
            }
            return campaign;
        }

        public async Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignRepositoryFacade.DeleteCampaignAsync(campaignId, ct);
        }

        private async Task<IList<CampaignProspect>> GetCampaignProspectsAsync(string campaignId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue($"CampaignProspects-{campaignId}", out IList<CampaignProspect> campaignProspects) == false)
            {
                campaignProspects = await _campaignRepositoryFacade.GetAllCampaignProspectsByCampaignIdAsync(campaignId);
                _memoryCache.Set($"CampaignProspects-{campaignId}", campaignProspects, TimeSpan.FromMinutes(3));
            }

            return campaignProspects;
        }
    }
}
