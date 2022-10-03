using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class FollowUpMessageCreateMQService : IFollowUpMessageCreateMQService
    {
        public FollowUpMessageCreateMQService(
            ILogger<FollowUpMessageCreateMQService> logger,
            ICampaignProspectRepository repository,
            IOptions<FeatureFlagsOptions> options,
            ITimestampService timestampService
            )
        {
            _featureFlagsOptions = options.Value;
            _repository = repository;
            _logger = logger;
            _timestampService = timestampService;
        }

        private readonly FeatureFlagsOptions _featureFlagsOptions;
        private readonly ICampaignProspectRepository _repository;
        private readonly ILogger<FollowUpMessageCreateMQService> _logger;
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

            if (prospect.FollowUpMessages.Count == 0)
            {
                await CreateProspectsFollowUpMessagesAsync(prospect, followUpMessages);
            }

            return await GenerateProspectMessagesAsync(halId, prospect, nextMessagesOrder, followUpMessages, ct);
        }

        private async Task CreateProspectsFollowUpMessagesAsync(CampaignProspect prospect, IList<FollowUpMessage> followUpMessages)
        {
            foreach (FollowUpMessage message in followUpMessages)
            {
                prospect.FollowUpMessages.Add(new()
                {
                    CampaignProspect = prospect,
                    CampaignProspectId = prospect.CampaignProspectId,
                    Order = message.Order,
                    Content = message.Content
                });

                await _repository.UpdateAsync(prospect);
            }
        }

        private async Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectMessagesAsync(string halId, CampaignProspect prospect, IList<int> nextMessagesOrder, IList<FollowUpMessage> followUpMessages, CancellationToken ct = default)
        {
            IList<CampaignProspectFollowUpMessage> prospectFollowUpMessages = new List<CampaignProspectFollowUpMessage>();
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
            CampaignProspectFollowUpMessage prospectFollowUpMessage = prospect.FollowUpMessages.SingleOrDefault(f => f.Order == message.Order);
            CampaignProspectFollowUpMessage previousCampaignProspectFollowUpMsg = GetPreviousFollowUpMessage(prospect.FollowUpMessages.ToList(), message.Order);
            prospectFollowUpMessage.PreviousFollowUpMessage = previousCampaignProspectFollowUpMsg;

            long? lastFollowUpMessageTimeStamp = DetermineLastFollowUpMessageTimeStamp(prospect, previousCampaignProspectFollowUpMsg);

            // do not send or generate any follow up messages.
            if (lastFollowUpMessageTimeStamp == null)
            {
                return null;
            }

            DateTimeOffset nextMessageDeliveryDateTime = GetFollowUpMessageDateTime(message.Delay, (long)lastFollowUpMessageTimeStamp);
            DateTimeOffset messageDeliveryDate = await _timestampService.GetLocalizedDateTimeOffsetAsync(halId, nextMessageDeliveryDateTime, ct);
            DateTimeOffset startWorkdayDate = await _timestampService.GetStartWorkdayLocalizedForHalIdAsync(halId, ct);
            DateTimeOffset endWorkdayDate = await _timestampService.GetEndWorkDayLocalizedForHalIdAsync(halId, ct);

            if (messageDeliveryDate < startWorkdayDate)
            {
                _logger.LogDebug("FollowUpMessage can be sent today and will be scheduled rightaway. Localized time when message is supposed to go out {0} and start of work day {1}.", messageDeliveryDate, startWorkdayDate);
                prospectFollowUpMessage.ExpectedDeliveryDateTime = messageDeliveryDate;
                prospectFollowUpMessage.ExpectedDeliveryDateTimeStamp = messageDeliveryDate.ToUnixTimeSeconds();
                await _repository.UpdateAsync(prospect);
            }
            else if (messageDeliveryDate > startWorkdayDate && messageDeliveryDate < endWorkdayDate)
            {
                if (_featureFlagsOptions.AllInOneVirtualAssistant == true)
                {
                    // check if messageDeliveryDate is in the past or in the future
                    // if it is in the past set properties on it, else set  prospectFollowUpMessage to null
                    DateTimeOffset now = await _timestampService.GetNowLocalizedAsync(halId, ct);
                    // messageDeliveryDate is in the past
                    if (messageDeliveryDate < now)
                    {
                        prospectFollowUpMessage.ExpectedDeliveryDateTime = messageDeliveryDate;
                        prospectFollowUpMessage.ExpectedDeliveryDateTimeStamp = messageDeliveryDate.ToUnixTimeSeconds();
                        await _repository.UpdateAsync(prospect);
                    }
                    else
                    {
                        prospectFollowUpMessage = null;
                    }
                }
                else
                {
                    _logger.LogDebug("FollowUpMessage will be sent out today. It is scheduled to go out at {0}", messageDeliveryDate);
                    prospectFollowUpMessage.ExpectedDeliveryDateTime = messageDeliveryDate;
                    prospectFollowUpMessage.ExpectedDeliveryDateTimeStamp = messageDeliveryDate.ToUnixTimeSeconds();
                    await _repository.UpdateAsync(prospect);
                }
            }
            else
            {
                _logger.LogDebug("FollowUpMessage will not be sent today. It falls outside of todays work hours. Start of work day is: {0} and end of work day is: {1}. Message time to go out is {3}", startWorkdayDate, endWorkdayDate, messageDeliveryDate);
            }

            return prospectFollowUpMessage;
        }

        private CampaignProspectFollowUpMessage GetPreviousFollowUpMessage(IList<CampaignProspectFollowUpMessage> followUpMessages, int currentOrder)
        {
            CampaignProspectFollowUpMessage previousFollowUpMessage = default;
            if (followUpMessages != null && followUpMessages.Count > 1)
            {
                if (currentOrder != 0)
                {

                    previousFollowUpMessage = followUpMessages[currentOrder - 1];
                }
            }

            return previousFollowUpMessage;
        }

        private long? DetermineLastFollowUpMessageTimeStamp(CampaignProspect prospect, CampaignProspectFollowUpMessage previouslySentMessage)
        {
            long? lastFollowUpMessageTimeStamp = null;
            if (previouslySentMessage != null)
            {
                if (previouslySentMessage.ActualDeliveryDateTimeStamp != 0)
                {
                    lastFollowUpMessageTimeStamp = previouslySentMessage.ActualDeliveryDateTimeStamp;
                }
                else
                {
                    // this means previous message was not yet delivered, in this scenario do not send the follow up message;
                    _logger.LogDebug("Previous follow up message was not delivered because 'ActualDeliveryDateTimeStamp' property is equal to zero. Wait until it has been delivered before scheduling the second one");
                }
            }
            else
            {
                if (prospect.AcceptedTimestamp == 0)
                {
                    _logger.LogWarning("This is the first follow up message that will be going out, we need to use AcceptedTimestamp time but it has not been set! Ensure it is set. Using now as the timestamp");
                    lastFollowUpMessageTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
                else
                {
                    lastFollowUpMessageTimeStamp = prospect.AcceptedTimestamp;
                }
            }

            return lastFollowUpMessageTimeStamp;
        }

        private DateTimeOffset GetFollowUpMessageDateTime(FollowUpMessageDelay delay, long timeStamp)
        {
            DateTimeOffset nextMessageDatetime = default;

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
