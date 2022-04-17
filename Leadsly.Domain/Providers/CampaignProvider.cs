using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CampaignProvider : ICampaignProvider
    {
        public CampaignProvider(
            ILogger<CampaignProvider> logger, 
            IMemoryCache memoryCache, 
            ICampaignService campaignService, 
            ICampaignManager campaignManager,
            IHalRepository halRepository,
            ITimestampService timestampService,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ICloudPlatformRepository cloudPlatformRepository       
            )
        {
            _memoryCache = memoryCache;
            _halRepository = halRepository;
            _campaignService = campaignService;
            _logger = logger;
            _timestampService = timestampService;
            _campaignManager = campaignManager;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _cloudPlatformRepository = cloudPlatformRepository;            
        }

        private readonly IMemoryCache _memoryCache;
        private readonly IHalRepository _halRepository;
        private readonly ITimestampService _timestampService;
        private readonly ILogger<CampaignProvider> _logger;        
        private readonly ICampaignManager _campaignManager;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;        
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ICampaignService _campaignService;        

        public void ProcessNewCampaign(Campaign campaign)
        {
            // ensure ScanForProspectReplies, ConnectionWithdraw and MonitorForNewProspects phases are running on hal
            // always trigger them here


            // if prospect list phase does not exists, this means were running new prospect list
            if(campaign.ProspectListPhase == null)
            {                
                _campaignManager.TriggerSendConnectionsPhase(campaign.CampaignId, campaign.ApplicationUserId);
            }
            else
            {
                _campaignManager.TriggerProspectListPhase(campaign.ProspectListPhase.ProspectListPhaseId, campaign.ApplicationUserId);
            }
        }

        public CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId)
        {
            CampaignProspectList campaignProspectList = _campaignService.GenerateCampaignProspectList(primaryProspectList, userId);

            return campaignProspectList;
        }

        public async Task<HalsProspectListPhasesPayload> GetActiveProspectListPhasesAsync(CancellationToken ct = default)
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

        public async Task<HalsProspectListPhasesPayload> GetIncompleteProspectListPhasesAsync(CancellationToken ct = default)
        {
            IList<ProspectListPhase> prospectListPhases = await _campaignRepositoryFacade.GetAllActiveProspectListPhasesAsync(ct);
            IList<ProspectListPhase> incompleteProspectListPhases = prospectListPhases.Where(p => p.Completed == false).ToList();

            IEnumerable<string> halIds = incompleteProspectListPhases.Select(phase => phase.Campaign.HalId).Distinct();

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

        public void TriggerSendConnectionsPhase(string campaignId, string userId)
        {
            _campaignManager.TriggerSendConnectionsPhase(campaignId, userId);
        }

        public void TriggerScanProspectsForRepliesPhase(string halId, string userId)
        {
            _campaignManager.TriggerScanProspectsForRepliesPhase(halId, userId);
        }

        public void TriggerFollowUpMessagesPhase(string halId, string userId)
        {
            _campaignManager.TriggerFollowUpMessagesPhase(halId, userId);
        }

        public async Task SendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            // grab first follow up messages for the following campaign id
            foreach (CampaignProspect campaignProspect in campaignProspects)
            {
                IList<FollowUpMessage> messages = await _campaignRepositoryFacade.GetFollowUpMessagesByCampaignIdAsync(campaignProspect.CampaignId, ct);
                if (messages != null)
                {
                    int nextFollowUpMessageOrder = DetermineNextFollowUpMessage(campaignProspect, messages);

                    if(nextFollowUpMessageOrder == 0)
                    {
                        // this campaign prospect has received all of the follow up messages configured
                        // check if the last follow up message was sent 14 or more days ago, if yes mark this prospect as complete or fullfilled
                    }
                    else
                    {
                        // else prepare next follow up message to be sent
                        FollowUpMessage messageToGoOut = messages.SingleOrDefault(m => m.Order == nextFollowUpMessageOrder);
                        await TriggerFollowUpMessageAsync(messageToGoOut, campaignProspect, ct);
                    }                    
                }
            }
        }

        private async Task<CampaignProspectFollowUpMessage> CreateCampaignProspectFollowUpMessageAsync(FollowUpMessage message, CampaignProspect campaignProspect, CancellationToken ct = default)
        {
            string content = message.Content.Replace("{{name}}", campaignProspect.Name);

            CampaignProspectFollowUpMessage followUpMessage = new()
            {
                CampaignProspect = campaignProspect,
                CampaignProspectId = campaignProspect.CampaignProspectId,
                Order = message.Order,
                Content = content
            };

            return await _campaignRepositoryFacade.CreateFollowUpMessageAsync(followUpMessage, ct);
        }

        private async Task TriggerFollowUpMessageAsync(FollowUpMessage message, CampaignProspect campaignProspect, CancellationToken ct = default)
        {
            string followUpMessageId = message.FollowUpMessageId;
            int order = message.Order;
            if (await CanFollowUpMessageBeSentTodayAsync(campaignProspect, message, ct) == true)
            {
                _logger.LogInformation("FollowUpMessage with id: {followUpMessageId} and order: {order}. Will be sent today", followUpMessageId, order);

                CampaignProspectFollowUpMessage campaignProspectFollowUpMessage = await CreateCampaignProspectFollowUpMessageAsync(message, campaignProspect, ct);

                DateTimeOffset followUpMessageDateTime = await GetFollowUpMessageDateTimeAsync(message, campaignProspect.Campaign.HalId, campaignProspect.LastFollowUpMessageSentTimestamp, ct);
                DateTimeOffset startWorkDayDateTime = await _timestampService.GetStartWorkDayAsync(campaignProspect.Campaign.HalId, ct);
                DateTimeOffset endWorkDateDateTime = await _timestampService.GetEndWorkDayAsync(campaignProspect.Campaign.HalId, ct);

                if (followUpMessageDateTime < startWorkDayDateTime)
                {
                    // fire right away
                    await _campaignManager.TriggerFollowUpMessagePhaseAsync(campaignProspectFollowUpMessage.CampaignProspectFollowUpMessageId, campaignProspect.CampaignId);
                }
                else if(followUpMessageDateTime > startWorkDayDateTime && followUpMessageDateTime < endWorkDateDateTime)
                {
                    // schedule with followUpMessageDateTime
                    await _campaignManager.TriggerFollowUpMessagePhaseAsync(campaignProspectFollowUpMessage.CampaignProspectFollowUpMessageId, campaignProspect.CampaignId, followUpMessageDateTime);
                }
            }
            else
            {
                // do nothing just log that the follow up messages falls outside of the work hours for hal                
                _logger.LogInformation("FollowUpMessage with id: {followUpMessageId} and order: {order}, falls outside of hal work hours and will not be sent today", followUpMessageId, order);
            }
        }

        private int DetermineNextFollowUpMessage(CampaignProspect campaignProspect, IList<FollowUpMessage> followUpMessages)
        {
            if(followUpMessages.Count == 0)
            {
                return -1;
            }

            if (campaignProspect.FollowUpMessageSent == true)
            {
                int previouslySentFollowUpMessageNum = campaignProspect.SentFollowUpMessageOrderNum;
                FollowUpMessage nextFollowUpMessage = followUpMessages.SingleOrDefault(f => f.Order == (previouslySentFollowUpMessageNum + 1));
                if(nextFollowUpMessage != null)
                {
                    return nextFollowUpMessage.Order;
                }
                else
                {
                    // if campaign has received all messages return 0
                    return 0;
                }
            }
            else
            {
                return 1;
            }
        }

        private async Task<DateTimeOffset> GetFollowUpMessageDateTimeAsync(FollowUpMessage message, string halId, long lastFollowUpMessageSentTimeStamp, CancellationToken ct = default)
        {
            DateTimeOffset followUpMessageDatetimeOffset = default;

            long timeStamp = lastFollowUpMessageSentTimeStamp;
            if (timeStamp == 0)
            {
                timeStamp = await _timestampService.CreateNowTimestampAsync(halId, ct);
            }

            switch (message.Delay.Unit)
            {
                case "MINUTES":
                    followUpMessageDatetimeOffset = DateTimeOffset.FromUnixTimeSeconds(timeStamp).AddMinutes(message.Delay.Value);
                    break;
                case "HOURS":
                    followUpMessageDatetimeOffset = DateTimeOffset.FromUnixTimeSeconds(timeStamp).AddHours(message.Delay.Value);
                    break;
                case "DAYS":
                    followUpMessageDatetimeOffset = DateTimeOffset.FromUnixTimeSeconds(timeStamp).AddDays(message.Delay.Value);
                    break;
                default:
                    break;
            }

            return followUpMessageDatetimeOffset;
        }

        private async Task<bool> CanFollowUpMessageBeSentTodayAsync(CampaignProspect campaignProspect, FollowUpMessage message, CancellationToken ct = default)
        {
            DateTimeOffset followUpMessageDatetimeOffset = await GetFollowUpMessageDateTimeAsync(message, campaignProspect.Campaign.HalId, campaignProspect.LastFollowUpMessageSentTimestamp, ct);

            DateTimeOffset startWorkDayDateTime = await _timestampService.GetStartWorkDayAsync(campaignProspect.Campaign.HalId, ct);
            DateTimeOffset endWorkDateDateTime = await _timestampService.GetEndWorkDayAsync(campaignProspect.Campaign.HalId, ct);

            bool canBeSentToday = false;
            // if followUpMessageDatetimeOffset falls between start date and end date schedule it
            if (followUpMessageDatetimeOffset < startWorkDayDateTime)
            {
                // schedule right away
                canBeSentToday = true;
            }
            else if(followUpMessageDatetimeOffset > startWorkDayDateTime && followUpMessageDatetimeOffset < endWorkDateDateTime)
            {
                // schedule it for the given date since it falls within our time range
                canBeSentToday = true;
            }
            else
            {
                // it is scheduled to go out sometime in the future don't do anything right now
                canBeSentToday = false;
            }

            return canBeSentToday;
        }
    }
}
