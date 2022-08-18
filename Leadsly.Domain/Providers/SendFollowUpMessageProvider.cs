using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
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
    public class SendFollowUpMessageProvider : ISendFollowUpMessageProvider
    {
        public SendFollowUpMessageProvider(
            ICampaignRepositoryFacade campaignRepositoryFacade,
            ITimestampService timestampService,
            IHangfireService hangfireService,
            ILogger<SendFollowUpMessageProvider> logger,
            IFollowUpMessageJobsRepository followUpMessageRepository,
            IMemoryCache memoryCache
            )
        {
            _hangfireService = hangfireService;
            _memoryCache = memoryCache;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _followUpMessageJobsRepository = followUpMessageRepository;
            _timestampService = timestampService;
            _logger = logger;
        }

        private readonly IHangfireService _hangfireService;
        private readonly IFollowUpMessageJobsRepository _followUpMessageJobsRepository;
        private readonly ILogger<SendFollowUpMessageProvider> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ITimestampService _timestampService;

        public async Task ScheduleFollowUpMessageAsync(FollowUpMessageBody followUpMessageBody, string queueNameIn, string routingKeyIn, string halId, DateTimeOffset scheduleTime, CancellationToken ct = default)
        {
            _logger.LogDebug($"Scheduling FollowUpMessage to be published at {scheduleTime}");
            string jobId = _hangfireService.Schedule<IFollowUpMessagePublisher>(x => x.PublishPhaseAsync(followUpMessageBody, queueNameIn, routingKeyIn, halId), scheduleTime);
            _logger.LogDebug($"Scheduled hangfire job id is {jobId}");

            FollowUpMessageJob followUpJob = new()
            {
                CampaignProspectId = followUpMessageBody.CampaignProspectId,
                HangfireJobId = jobId,
                FollowUpMessageId = followUpMessageBody.FollowUpMessageId
            };

            await _followUpMessageJobsRepository.AddFollowUpJobAsync(followUpJob, ct);
        }

        public async Task<IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset>> CreateSendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset> goingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();

            // grab first follow up messages for the following campaign id
            foreach (CampaignProspect campaignProspect in campaignProspects)
            {
                IList<FollowUpMessage> messages = await GetFollowUpMessagesByCampaignIdAsync(campaignProspect.CampaignId, ct);
                if (messages != null && messages.Count != 0)
                {
                    IList<int> nextFollowUpMessageOrders = DetermineNextFollowUpMessages(campaignProspect, messages);
                    if (nextFollowUpMessageOrders.Count != 0)
                    {
                        // this campaign prospect has received all of the follow up messages configured
                        // check if the last follow up message was sent 14 or more days ago, if yes mark this prospect as complete or fullfilled
                        DateTimeOffset localLastFollowUpMessage = await _timestampService.GetDateFromTimestampLocalizedAsync(campaignProspect.Campaign.HalId, campaignProspect.LastFollowUpMessageSentTimestamp);

                        _logger.LogDebug($"[CreateSendFollowUpMessagesAsync]: Last follow up message localized date time is {localLastFollowUpMessage}");
                        DateTimeOffset localNow = await _timestampService.GetNowLocalizedAsync(campaignProspect.Campaign.HalId);
                        _logger.LogDebug($"[CreateSendFollowUpMessagesAsync]: Localized DateTime now is {localNow}");
                        if ((localLastFollowUpMessage - localNow).TotalDays < 14)
                        {
                            campaignProspect.FollowUpComplete = true;
                            await _campaignRepositoryFacade.UpdateCampaignProspectAsync(campaignProspect, ct);
                        }
                    }
                    else
                    {
                        foreach (int nextFollowUpMessageOrder in nextFollowUpMessageOrders)
                        {
                            // else prepare next follow up message to be sent
                            FollowUpMessage messageToGoOut = messages.SingleOrDefault(m => m.Order == nextFollowUpMessageOrder);
                            var messagesOut = await CreateFollowUpMessagesAsync(messageToGoOut, campaignProspect, ct);
                            goingOut.AddRange(messagesOut);
                        }
                    }
                }
            }

            return goingOut;
        }

        private async Task<IList<FollowUpMessage>> GetFollowUpMessagesByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(campaignId, out IList<FollowUpMessage> followUpMessages) == false)
            {
                followUpMessages = await _campaignRepositoryFacade.GetFollowUpMessagesByCampaignIdAsync(campaignId, ct);
                _memoryCache.Set(campaignId, followUpMessages);
            }
            return followUpMessages;
        }

        private async Task<Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>> CreateFollowUpMessagesAsync(FollowUpMessage message, CampaignProspect campaignProspect, CancellationToken ct = default)
        {
            string followUpMessageId = message.FollowUpMessageId;
            int order = message.Order;
            Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset> goingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();
            if (await CanFollowUpMessageBeSentTodayAsync(campaignProspect, message, ct) == true)
            {
                _logger.LogInformation("FollowUpMessage with id: {followUpMessageId} and order: {order}. Will be sent today", followUpMessageId, order);

                // check if this prospect already contains a follow up message created with the given order, so we don't needlessly create multiple follow up messages
                // and so that multiple follow up messages dont go out
                CampaignProspectFollowUpMessage campaignProspectFollowUpMessage = await CreateOrGetFollowUpMessageAsync(message, campaignProspect, ct);

                DateTimeOffset followUpMessageDateTime = GetFollowUpMessageDateTime(message, campaignProspect.LastFollowUpMessageSentTimestamp);
                _logger.LogDebug($"[CreateFollowUpMessagesAsync]: Non localized date time to send out follow up message is: {followUpMessageDateTime}");

                DateTimeOffset followUpmessageDateTimeOffsetLocalized = await _timestampService.GetLocalizedDateTimeOffsetAsync(campaignProspect.Campaign.HalId, followUpMessageDateTime, ct);
                _logger.LogDebug($"[CreateFollowUpMessagesAsync]: Localized date time to send out follow up message is: {followUpmessageDateTimeOffsetLocalized}");
                DateTimeOffset startWorkDayDateTimeOffset = await _timestampService.GetStartWorkdayLocalizedForHalIdAsync(campaignProspect.Campaign.HalId, ct);
                DateTimeOffset endWorkDateDateTimeOffset = await _timestampService.GetEndWorkDayLocalizedForHalIdAsync(campaignProspect.Campaign.HalId, ct);

                if (followUpmessageDateTimeOffsetLocalized < startWorkDayDateTimeOffset)
                {
                    goingOut.Add(campaignProspectFollowUpMessage, default);
                }
                else if (followUpmessageDateTimeOffsetLocalized > startWorkDayDateTimeOffset && followUpmessageDateTimeOffsetLocalized < endWorkDateDateTimeOffset)
                {
                    goingOut.Add(campaignProspectFollowUpMessage, followUpmessageDateTimeOffsetLocalized);
                }
            }
            else
            {
                // do nothing just log that the follow up messages falls outside of the work hours for hal                
                _logger.LogInformation("FollowUpMessage with id: {followUpMessageId} and order: {order}, falls outside of hal work hours and will not be sent today", followUpMessageId, order);
            }

            return goingOut;
        }

        private async Task<CampaignProspectFollowUpMessage> CreateOrGetFollowUpMessageAsync(FollowUpMessage message, CampaignProspect campaignProspect, CancellationToken ct = default)
        {
            if (campaignProspect.FollowUpMessages.Any(f => f.Order == message.Order) == true)
            {
                // if for whatever reason the server was restarted or we have already created a follow up message for this prospect
                // with the given order id
                return campaignProspect.FollowUpMessages.SingleOrDefault(f => f.Order == message.Order);
            }
            else
            {
                string firstName = campaignProspect.Name.Split(' ').FirstOrDefault();
                firstName = string.IsNullOrEmpty(firstName) ? "there" : firstName.Capitalize();
                string content = message.Content.Replace("{firstName}", firstName);

                CampaignProspectFollowUpMessage followUpMessage = new()
                {
                    CampaignProspect = campaignProspect,
                    CampaignProspectId = campaignProspect.CampaignProspectId,
                    Order = message.Order,
                    Content = content
                };

                return await _campaignRepositoryFacade.CreateFollowUpMessageAsync(followUpMessage, ct);
            }
        }

        private DateTimeOffset GetFollowUpMessageDateTime(FollowUpMessage message, long lastFollowUpMessageSentTimeStamp)
        {
            DateTimeOffset followUpMessageDatetimeOffset = default;

            long timeStamp = lastFollowUpMessageSentTimeStamp;
            if (timeStamp == 0)
            {
                timeStamp = _timestampService.CreateNowTimestamp();
            }

            switch (message.Delay.Unit.ToUpper())
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
            DateTimeOffset followUpMessageDatetimeOffset = GetFollowUpMessageDateTime(message, campaignProspect.LastFollowUpMessageSentTimestamp);

            DateTimeOffset followUpMessageDateTimeOffsetLocalized = await _timestampService.GetLocalizedDateTimeOffsetAsync(campaignProspect.Campaign.HalId, followUpMessageDatetimeOffset, ct);
            _logger.LogDebug($"[CanFollowUpMessageBeSentTodayAsync]: Localized follow up message date time offset is {followUpMessageDateTimeOffsetLocalized}");
            DateTimeOffset startWorkDayDateTime = await _timestampService.GetStartWorkdayLocalizedForHalIdAsync(campaignProspect.Campaign.HalId, ct);
            DateTimeOffset endWorkDateDateTime = await _timestampService.GetEndWorkDayLocalizedForHalIdAsync(campaignProspect.Campaign.HalId, ct);

            bool canBeSentToday = false;
            // if followUpMessageDatetimeOffset falls between start date and end date schedule it
            if (followUpMessageDateTimeOffsetLocalized < startWorkDayDateTime)
            {
                // schedule right away
                canBeSentToday = true;
            }
            else if (followUpMessageDateTimeOffsetLocalized > startWorkDayDateTime && followUpMessageDateTimeOffsetLocalized < endWorkDateDateTime)
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

        private IList<int> DetermineNextFollowUpMessages(CampaignProspect campaignProspect, IList<FollowUpMessage> followUpMessages)
        {
            // the order ids of the next follow up messages.
            IList<int> followUpOrders = new List<int>();
            if (followUpMessages.Count == 0)
            {
                return followUpOrders;
            }

            if (campaignProspect.FollowUpMessageSent == true)
            {
                int previouslySentFollowUpMessageNum = campaignProspect.SentFollowUpMessageOrderNum;
                followUpOrders = followUpMessages.Where(x => x.Order != previouslySentFollowUpMessageNum).Select(x => x.Order).ToList();
            }
            else
            {
                followUpOrders = followUpMessages.Select(m => m.Order).ToList();
            }

            return followUpOrders;
        }

        private int DetermineNextFollowUpMessage(CampaignProspect campaignProspect, IList<FollowUpMessage> followUpMessages)
        {
            if (followUpMessages.Count == 0)
            {
                return -1;
            }

            if (campaignProspect.FollowUpMessageSent == true)
            {
                int previouslySentFollowUpMessageNum = campaignProspect.SentFollowUpMessageOrderNum;
                FollowUpMessage nextFollowUpMessage = followUpMessages.SingleOrDefault(f => f.Order == (previouslySentFollowUpMessageNum + 1));
                if (nextFollowUpMessage != null)
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
    }
}
