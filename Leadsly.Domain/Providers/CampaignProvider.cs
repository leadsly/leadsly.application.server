using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Requests;
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
            ICampaignService campaignService,
            ITimestampService timestampService,
            ICampaignPhaseClient campaignPhaseClient,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IHangfireService hangfireService,
            IMemoryCache memoryCache,
            IFollowUpMessageJobsRepository followUpMessageJobRepository,
            ISocialAccountRepository socialAccountRepository
            )
        {
            _hangfireService = hangfireService;
            _followUpMessageJobRepository = followUpMessageJobRepository;
            _memoryCache = memoryCache;
            _socialAccountRepository = socialAccountRepository;
            _campaignPhaseClient = campaignPhaseClient;
            _timestampService = timestampService;
            _campaignService = campaignService;
            _logger = logger;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IHangfireService _hangfireService;
        private readonly IFollowUpMessageJobsRepository _followUpMessageJobRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICampaignPhaseClient _campaignPhaseClient;
        private readonly ITimestampService _timestampService;
        private readonly ILogger<CampaignProvider> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ICampaignService _campaignService;

        [Obsolete]
        public CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId)
        {
            CampaignProspectList campaignProspectList = _campaignService.GenerateCampaignProspectList(primaryProspectList, userId);

            return campaignProspectList;
        }

        private async Task<HalsProspectListPhasesPayload> GetActiveProspectListPhasesAsync(CancellationToken ct = default)
        {
            IList<ProspectListPhase> prospectListPhases = await _campaignRepositoryFacade.GetAllActiveProspectListPhasesAsync(ct);

            IEnumerable<string> halIds = prospectListPhases.Select(phase => phase.Campaign.HalId).Distinct();

            HalsProspectListPhasesPayload halsPhases = new();
            foreach (string halId in halIds)
            {
                List<ProspectListBody> content = prospectListPhases.Where(p => p.Campaign.HalId == halId).Select(p =>
                {
                    return new ProspectListBody
                    {
                        SearchUrls = p.SearchUrls
                    };
                }).ToList();

                halsPhases.ProspectListPayload.Add(halId, content);
            }

            return halsPhases;
        }

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

        public async Task<List<string>> GetHalIdsWithActiveCampaignsAsync(CancellationToken ct = default)
        {
            IList<Campaign> activeCampaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsAsync(ct);

            List<string> halIds = activeCampaigns.Select(c => c.HalId).ToList();

            return halIds;
        }

        public async Task<int> CreateDailyWarmUpLimitConfigurationAsync(long startDateTimestamp, CancellationToken ct = default)
        {
            return await _campaignService.CreateDailyWarmUpLimitConfigurationAsync(startDateTimestamp, ct);
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

        [Obsolete]
        public async Task<HalOperationResultViewModel<T>> CreateCampaignAsync<T>(CreateCampaignRequest request, string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            Campaign newCampaign = new()
            {
                Name = request.CampaignDetails.Name,
                StartTimestamp = request.CampaignDetails.StartTimestamp,
                EndTimestamp = request.CampaignDetails.EndTimestamp,
                DailyInvites = request.CampaignDetails.DailyInviteLimit,
                IsWarmUpEnabled = request.CampaignDetails.WarmUp,
                CampaignType = request.CampaignDetails.CampaignType,
                FollowUpMessages = new List<FollowUpMessage>(),
                ApplicationUserId = userId
            };

            // check if we're using existing prospect list or not
            PrimaryProspectList primaryProspectList = default;
            if (request.CampaignDetails.PrimaryProspectList.Existing == true)
            {
                // grab existing primary prospect list by prospect list name and user id
                primaryProspectList = await _campaignRepositoryFacade.GetPrimaryProspectListByNameAndUserIdAsync(request.CampaignDetails.PrimaryProspectList.Name, userId, ct);
            }
            else
            {
                primaryProspectList = new()
                {
                    Name = request.CampaignDetails.PrimaryProspectList.Name,
                    SearchUrls = new List<SearchUrl>(),
                    UserId = userId,
                    CreatedTimestamp = _timestampService.CreateNowTimestamp()
                };

                foreach (string searchUrl in request.CampaignDetails.PrimaryProspectList.SearchUrls)
                {
                    primaryProspectList.SearchUrls.Add(new()
                    {
                        PrimaryProspectList = primaryProspectList,
                        Url = searchUrl
                    });
                }

                primaryProspectList = await _campaignRepositoryFacade.CreatePrimaryProspectListAsync(primaryProspectList, ct);
            }

            CampaignProspectList campaignProspectList = CreateCampaignProspectList(primaryProspectList, userId);
            newCampaign.CampaignProspectList = campaignProspectList;

            foreach (FollowUpMessageViewModel followUpMsg in request.FollowUpMessages)
            {
                FollowUpMessage followUpMessage = new()
                {
                    Campaign = newCampaign,
                    Content = followUpMsg.Content,
                    Order = followUpMsg.Order,
                    Delay = new()
                    {
                        Unit = followUpMsg.Delay.Unit,
                        Value = followUpMsg.Delay.Value
                    }
                };

                newCampaign.FollowUpMessages.Add(followUpMessage);
            }

            newCampaign.HalId = request.HalId;

            Campaign newCampaignWithPhases = CreateCampaignPhases(newCampaign, request.CampaignDetails.PrimaryProspectList.Existing, ct);

            IList<SearchUrlDetails> sentConnectionsSearchUrlStatuses = new List<SearchUrlDetails>();
            IList<Application.Model.Entities.Campaigns.SearchUrlProgress> searchUrlsProgress = new List<Application.Model.Entities.Campaigns.SearchUrlProgress>();
            IList<string> searchUrls = request.CampaignDetails.PrimaryProspectList.Existing ? primaryProspectList.SearchUrls.Select(s => s.Url).ToList() : request.CampaignDetails.PrimaryProspectList.SearchUrls;
            foreach (string searchUrl in searchUrls)
            {
                SearchUrlDetails sentConnectionsSearchUrlStatus = new()
                {
                    Campaign = newCampaignWithPhases,
                    CurrentUrl = searchUrl,
                    FinishedCrawling = false,
                    StartedCrawling = false,
                    OriginalUrl = searchUrl,
                    WindowHandleId = string.Empty,
                    LastActivityTimestamp = 0
                };
                sentConnectionsSearchUrlStatuses.Add(sentConnectionsSearchUrlStatus);

                Application.Model.Entities.Campaigns.SearchUrlProgress searchUrlProgress = new()
                {
                    Campaign = newCampaignWithPhases,
                    LastPage = 1,
                    LastProcessedProspect = 0,
                    TotalSearchResults = 0,
                    SearchUrl = searchUrl,
                    WindowHandleId = string.Empty
                };
                searchUrlsProgress.Add(searchUrlProgress);
            }
            newCampaignWithPhases.SentConnectionsStatuses = sentConnectionsSearchUrlStatuses;
            newCampaignWithPhases.SearchUrlsProgress = searchUrlsProgress;

            if (request.CampaignDetails.WarmUp == true)
            {
                // create warmup configuration
                await CreateDailyWarmUpLimitConfigurationAsync(request.CampaignDetails.StartTimestamp, ct);
            }

            // create send connections stages. For when each campaign will send out its connections
            newCampaignWithPhases.SendConnectionStages = CreateSendConnectionsStages();

            // create new ProspectList if we're not using an existing one
            newCampaignWithPhases = await _campaignRepositoryFacade.CreateCampaignAsync(newCampaignWithPhases, ct);
            if (newCampaignWithPhases == null)
            {
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.DATABASE_OPERATION_ERROR,
                    Reason = "Failed to create new campaign",
                    Detail = "Error occured while saving the new campaign and it's phases"
                });
                return result;
            }

            SocialAccount socialAccount = await _socialAccountRepository.GetByUserNameAsync(request.ConnectedAccount, ct);
            if (socialAccount.RunProspectListFirst)
            {
                await _campaignPhaseClient.HandleNewCampaignAsync(newCampaign);
            }
            else
            {
                await _campaignPhaseClient.HandleNewCampaignMergedAsync(newCampaign);
            }

            result.OperationResults.Succeeded = true;
            result.Data = newCampaignWithPhases;
            return result;
        }

        [Obsolete]
        private IList<SendConnectionsStage> CreateSendConnectionsStages()
        {
            IList<SendConnectionsStage> connectionsStages = new List<SendConnectionsStage>();
            for (int i = 0; i < 3; i++)
            {
                SendConnectionsStage sendConnectionsStage = new();
                if (i == 0)
                {
                    sendConnectionsStage = new SendConnectionsStage
                    {
                        StartTime = "8:00 AM",
                        Order = i + 1
                    };
                }
                else if (i == 1)
                {
                    sendConnectionsStage = new SendConnectionsStage
                    {
                        StartTime = "12:00 PM",
                        Order = i + 1
                    };
                }
                else
                {
                    sendConnectionsStage = new SendConnectionsStage
                    {
                        StartTime = "5:00PM",
                        Order = i + 1
                    };
                }

                connectionsStages.Add(sendConnectionsStage);
            }

            return connectionsStages;
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

        public async Task<IList<Campaign>> GetAllActiveByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _campaignRepositoryFacade.GetAllActiveCampaignsByUserIdAsync(userId, ct);
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

        [Obsolete]
        private Campaign CreateCampaignPhases(Campaign newCampaign, bool useExistingProspectList, CancellationToken ct = default)
        {
            CampaignType campaignType = default;
            switch (newCampaign.CampaignType)
            {
                case CampaignTypeEnum.None:
                    break;
                case CampaignTypeEnum.Invitations:
                    campaignType = new InvitationsCampaign();
                    break;
                case CampaignTypeEnum.FollowUp:
                    campaignType = new FollowUpCampaign();
                    break;
                case CampaignTypeEnum.ProfileVisits:
                    break;
                default:
                    break;
            }

            Campaign campaign = campaignType.GeneratePhases(newCampaign, useExistingProspectList);

            return campaign;
        }
    }
}
