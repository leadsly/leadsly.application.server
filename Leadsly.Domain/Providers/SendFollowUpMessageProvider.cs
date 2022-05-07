﻿using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
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
    public class SendFollowUpMessageProvider : ISendFollowUpMessageProvider
    {
        public SendFollowUpMessageProvider(
            ICampaignRepositoryFacade campaignRepositoryFacade, 
            ITimestampService timestampService, 
            ILogger<SendFollowUpMessageProvider> logger,
            IMemoryCache memoryCache
            )
        {
            _memoryCache = memoryCache;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _timestampService = timestampService;
            _logger = logger;
        }

        private readonly ILogger<SendFollowUpMessageProvider> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ITimestampService _timestampService;

        public async Task<IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset>> CreateSendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset> goingOut = new Dictionary<CampaignProspectFollowUpMessage, DateTimeOffset>();

            // grab first follow up messages for the following campaign id
            foreach (CampaignProspect campaignProspect in campaignProspects)
            {
                IList<FollowUpMessage> messages = await GetFollowUpMessagesByCampaignIdAsync(campaignProspect.CampaignId, ct);
                if (messages != null)
                {
                    IList<int> nextFollowUpMessageOrders = DetermineNextFollowUpMessages(campaignProspect, messages);
                    if(nextFollowUpMessageOrders.Count == 0)
                    {
                        // this campaign prospect has received all of the follow up messages configured
                        // check if the last follow up message was sent 14 or more days ago, if yes mark this prospect as complete or fullfilled
                        DateTimeOffset lastFollowUpMessage = _timestampService.GetDateFromTimestamp(campaignProspect.LastFollowUpMessageSentTimestamp);
                        if ((lastFollowUpMessage.LocalDateTime - DateTime.Now).TotalDays < 14)
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
            if(_memoryCache.TryGetValue(campaignId, out IList<FollowUpMessage> followUpMessages) == false)
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

                CampaignProspectFollowUpMessage campaignProspectFollowUpMessage = await CreateCampaignProspectFollowUpMessageAsync(message, campaignProspect, ct);

                DateTimeOffset followUpMessageDateTime = GetFollowUpMessageDateTime(message, campaignProspect.LastFollowUpMessageSentTimestamp);

                DateTime followUpmessageDateTimeLocalized = await _timestampService.GetLocalizedDateTimeAsync(campaignProspect.Campaign.HalId, followUpMessageDateTime, ct);
                DateTime startWorkDayDateTime = await _timestampService.GetStartWorkDayLocalizedAsync(campaignProspect.Campaign.HalId, ct);
                DateTime endWorkDateDateTime = await _timestampService.GetEndWorkDayLocalizedAsync(campaignProspect.Campaign.HalId, ct);

                if (followUpmessageDateTimeLocalized < startWorkDayDateTime)
                {
                    goingOut.Add(campaignProspectFollowUpMessage, default);
                }
                else if (followUpmessageDateTimeLocalized > startWorkDayDateTime && followUpmessageDateTimeLocalized < endWorkDateDateTime)
                {
                    goingOut.Add(campaignProspectFollowUpMessage, followUpMessageDateTime);
                }
            }
            else
            {
                // do nothing just log that the follow up messages falls outside of the work hours for hal                
                _logger.LogInformation("FollowUpMessage with id: {followUpMessageId} and order: {order}, falls outside of hal work hours and will not be sent today", followUpMessageId, order);
            }

            return goingOut;
        }

        private async Task<CampaignProspectFollowUpMessage> CreateCampaignProspectFollowUpMessageAsync(FollowUpMessage message, CampaignProspect campaignProspect, CancellationToken ct = default)
        {
            string content = message.Content.Replace("{firstName}", campaignProspect.Name);

            CampaignProspectFollowUpMessage followUpMessage = new()
            {
                CampaignProspect = campaignProspect,
                CampaignProspectId = campaignProspect.CampaignProspectId,
                Order = message.Order,
                Content = content
            };

            return await _campaignRepositoryFacade.CreateFollowUpMessageAsync(followUpMessage, ct);
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

            DateTime followUpMessageDateTimeLocalized = await _timestampService.GetLocalizedDateTimeAsync(campaignProspect.Campaign.HalId, followUpMessageDatetimeOffset, ct);
            DateTime startWorkDayDateTime = await _timestampService.GetStartWorkDayLocalizedAsync(campaignProspect.Campaign.HalId, ct);
            DateTime endWorkDateDateTime = await _timestampService.GetEndWorkDayLocalizedAsync(campaignProspect.Campaign.HalId, ct);

            bool canBeSentToday = false;
            // if followUpMessageDatetimeOffset falls between start date and end date schedule it
            if (followUpMessageDateTimeLocalized < startWorkDayDateTime)
            {
                // schedule right away
                canBeSentToday = true;
            }
            else if (followUpMessageDateTimeLocalized > startWorkDayDateTime && followUpMessageDateTimeLocalized < endWorkDateDateTime)
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
            if(followUpMessages.Count == 0)
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
