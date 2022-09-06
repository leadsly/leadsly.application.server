using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class FollowUpMessagesProvider : IFollowUpMessagesProvider
    {
        public FollowUpMessagesProvider(
            ILogger<FollowUpMessagesProvider> logger,
            IFollowUpMessagesService service,
            ITimestampService timestampService,
            IFollowUpMessagePublisherService publishingService
            )
        {
            _publishingService = publishingService;
            _timestampService = timestampService;
            _logger = logger;
            _service = service;
        }

        private readonly IFollowUpMessagePublisherService _publishingService;
        private readonly ITimestampService _timestampService;
        private readonly ILogger<FollowUpMessagesProvider> _logger;
        private readonly IFollowUpMessagesService _service;

        public async Task PublishMessageAsync(string halId, CancellationToken ct = default)
        {
            IList<PublishMessageBody> mqMessages = await _service.CreateMQFollowUpMessagesAsync(halId);

            foreach (PublishMessageBody mqMessage in mqMessages)
            {
                await PublishMessageAsync(mqMessage);
            }
        }

        protected async Task PublishMessageAsync(PublishMessageBody message)
        {
            string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
            string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
            string halId = message.HalId;
            FollowUpMessageBody followUpMessage = message as FollowUpMessageBody;

            DateTimeOffset nowLocalized = await _timestampService.GetNowLocalizedAsync(message.HalId);
            if (followUpMessage.ExpectedDeliveryDateTime < nowLocalized)
            {
                _logger.LogInformation("FollowUpMessageBody does not have a schedule time set, sending message immediately");
                _publishingService.PublishMessage(message, queueNameIn, routingKeyIn, halId);
            }
            else
            {
                _logger.LogInformation($"Scheduling FollowUpMessageBody to go out at {followUpMessage.ExpectedDeliveryDateTime}");
                await _publishingService.ScheduleMessageAsync(message, queueNameIn, routingKeyIn, halId);
            }
        }
    }
}
