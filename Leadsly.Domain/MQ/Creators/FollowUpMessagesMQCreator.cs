using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators
{
    public class FollowUpMessagesMQCreator : IFollowUpMessagesMQCreator
    {
        public FollowUpMessagesMQCreator(
            ILogger<FollowUpMessagesMQCreator> logger,
            IFollowUpMessagesMQService service,
            ITimestampService timestampService,
            IFollowUpMessageMQPublisherService publishingService
            )
        {
            _publishingService = publishingService;
            _timestampService = timestampService;
            _logger = logger;
            _service = service;
        }

        private readonly IFollowUpMessageMQPublisherService _publishingService;
        private readonly ITimestampService _timestampService;
        private readonly ILogger<FollowUpMessagesMQCreator> _logger;
        private readonly IFollowUpMessagesMQService _service;

        public async Task PublishMessageAsync(string halId, CancellationToken ct = default)
        {
            IList<PublishMessageBody> mqMessages = await _service.CreateMQFollowUpMessagesAsync(halId);

            await PublishMessagesToRabbitMQAsync(mqMessages);
        }

        public async Task PublishMessageAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default)
        {
            IList<PublishMessageBody> mqMessages = await _service.CreateMQFollowUpMessagesAsync(halId, campaigns, ct);

            await PublishMessagesToRabbitMQAsync(mqMessages);
        }

        private async Task PublishMessagesToRabbitMQAsync(IList<PublishMessageBody> mqMessages)
        {
            foreach (PublishMessageBody mqMessage in mqMessages)
            {
                await PublishMessageAsync(mqMessage);
            }
        }

        private async Task PublishMessageAsync(PublishMessageBody message)
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
