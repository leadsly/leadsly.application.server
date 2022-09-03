using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class CreateFollowUpMessageService : ICreateFollowUpMessageService
    {
        public CreateFollowUpMessageService(
            ILogger<CreateFollowUpMessageService> logger,
            ITimestampService timestampService
            )
        {
            _logger = logger;
            _timestampService = timestampService;
        }

        private readonly ILogger<CreateFollowUpMessageService> _logger;
        private readonly ITimestampService _timestampService;

        public async Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectFollowUpMessagesAsync(string halId, CampaignProspect prospect, IList<FollowUpMessage> followUpMessages, CancellationToken ct = default)
        {
            if (followUpMessages == null || followUpMessages.Count == 0)
            {
                return null;
            }

            IList<int> nextMessagesOrder = DetermineNextFollowUpMessages(prospect, followUpMessages);
            if (nextMessagesOrder.Count == 0)
            {
                // this means we have no more follow up messages to send
                return null;
            }

            return await GenerateProspectMessagesAsync(halId, prospect, nextMessagesOrder, followUpMessages, ct);
        }

        private async Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectMessagesAsync(string halId, CampaignProspect prospect, IList<int> nextMessagesOrder, IList<FollowUpMessage> followUpMessages, CancellationToken ct = default)
        {
            IList<Leadsly.Domain.Models.Entities.Campaigns.CampaignProspectFollowUpMessage> prospectFollowUpMessages = new List<CampaignProspectFollowUpMessage>();
            foreach (int nextMessageOrder in nextMessagesOrder)
            {
                FollowUpMessage followUpMessage = followUpMessages.FirstOrDefault(m => m.Order == nextMessageOrder);

                CampaignProspectFollowUpMessage prospectFollowUpMessage = await GenerateProspectMessageAsync(halId, prospect, followUpMessage, ct);
                if (prospectFollowUpMessage != null)
                {
                    prospectFollowUpMessages.Add(prospectFollowUpMessage);
                }
            }

            return prospectFollowUpMessages;
        }

        private async Task<CampaignProspectFollowUpMessage> GenerateProspectMessageAsync(string halId, CampaignProspect prospect, FollowUpMessage message, CancellationToken ct = default)
        {
            DateTimeOffset lastMessageDeliveryDate = GetFollowUpMessageDateTime(message.Delay, prospect.LastFollowUpMessageSentTimestamp);
            DateTimeOffset messageDeliveryDate = await _timestampService.GetLocalizedDateTimeOffsetAsync(halId, lastMessageDeliveryDate, ct);
            DateTimeOffset startWorkdayDate = await _timestampService.GetStartWorkdayLocalizedForHalIdAsync(halId, ct);
            DateTimeOffset endWorkdayDate = await _timestampService.GetEndWorkDayLocalizedForHalIdAsync(halId, ct);

            CampaignProspectFollowUpMessage prospectFollowUpMessage = default;
            if (messageDeliveryDate < startWorkdayDate)
            {
                _logger.LogDebug("FollowUpMessage can be sent today and will be scheduled rightaway. Localized time when message is supposed to go out {0} and start of work day {1}.", messageDeliveryDate, startWorkdayDate);
                prospectFollowUpMessage = CreateProspectFollowUpMessage(message, prospect, messageDeliveryDate);
            }
            else if (messageDeliveryDate > startWorkdayDate && messageDeliveryDate < endWorkdayDate)
            {
                _logger.LogDebug("FollowUpMessage will be sent out today. It is scheduled to go out at {0}", messageDeliveryDate);
                // schedule it for the given date since it falls within our time range
                prospectFollowUpMessage = CreateProspectFollowUpMessage(message, prospect, messageDeliveryDate);
            }
            else
            {
                _logger.LogDebug("FollowUpMessage will not be sent today. It falls outside of todays work hours. Start of work day is: {0} and end of work day is: {1}. Message time to go out is {3}", startWorkdayDate, endWorkdayDate, messageDeliveryDate);
            }

            return prospectFollowUpMessage;
        }

        private CampaignProspectFollowUpMessage CreateProspectFollowUpMessage(FollowUpMessage message, CampaignProspect prospect, DateTimeOffset expectedDeliveryDateTime)
        {
            if (prospect.FollowUpMessages.Any(f => f.Order == message.Order) == true)
            {
                // if for whatever reason the server was restarted or we have already created a follow up message for this prospect
                // with the given order id
                return prospect.FollowUpMessages.SingleOrDefault(f => f.Order == message.Order);
            }
            else
            {
                return new()
                {
                    CampaignProspect = prospect,
                    CampaignProspectId = prospect.CampaignProspectId,
                    Order = message.Order,
                    Content = message.Content,
                    ExpectedDeliveryDateTime = expectedDeliveryDateTime,
                    ExpectedDeliveryDateTimeStamp = expectedDeliveryDateTime.ToUnixTimeSeconds()
                };
            }
        }

        private DateTimeOffset GetFollowUpMessageDateTime(FollowUpMessageDelay delay, long lastMessageTimestamp)
        {
            DateTimeOffset nextMessageDatetime = default;
            long timeStamp = lastMessageTimestamp;
            if (timeStamp == 0)
            {
                _logger.LogDebug("FollowUpMessage does not have a last message timestamp. This means this is a new message that will be going out.");
                timeStamp = _timestampService.CreateNowTimestamp();
            }

            switch (delay.Unit.ToUpper())
            {
                case "MINUTES":
                    nextMessageDatetime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).AddMinutes(delay.Value);
                    break;
                case "HOURS":
                    nextMessageDatetime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).AddHours(delay.Value);
                    break;
                case "DAYS":
                    nextMessageDatetime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).AddDays(delay.Value);
                    break;
                default:
                    break;
            }

            _logger.LogDebug("FollowUpMessage delay is {0} {1}", delay.Value, delay.Unit);
            return nextMessageDatetime;
        }

        private IList<int> DetermineNextFollowUpMessages(CampaignProspect prospect, IList<FollowUpMessage> followUpMessages)
        {
            IList<int> nextMessages = new List<int>();

            if (prospect.FollowUpMessageSent == true)
            {
                int previouslySentFollowUpMessageNum = prospect.SentFollowUpMessageOrderNum;
                nextMessages = followUpMessages.Where(x => x.Order > previouslySentFollowUpMessageNum).OrderBy(x => x.Order).Select(x => x.Order).ToList();
            }
            else
            {
                nextMessages = followUpMessages.OrderBy(x => x.Order).Select(m => m.Order).ToList();
            }

            return nextMessages;
        }
    }
}
