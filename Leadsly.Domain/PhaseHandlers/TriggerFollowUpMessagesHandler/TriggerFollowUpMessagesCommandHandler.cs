using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.RabbitMQMessages;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers.TriggerFollowUpMessagesHandler
{
    public class TriggerFollowUpMessagesCommandHandler : TriggerPhaseBase, ICommandHandler<TriggerFollowUpMessagesCommand>
    {
        public TriggerFollowUpMessagesCommandHandler(
            IFollowUpMessagesProvider provider,
            IFollowUpMessagePublisherService publishingService,
            ITimestampService timestampService,
            ILogger<TriggerFollowUpMessagesCommandHandler> logger)
        {
            _publishingService = publishingService;
            _provider = provider;
            _timestampService = timestampService;
            _logger = logger;
        }

        private readonly ITimestampService _timestampService;
        private readonly ILogger<TriggerFollowUpMessagesCommandHandler> _logger;
        private readonly IFollowUpMessagePublisherService _publishingService;
        private readonly IFollowUpMessagesProvider _provider;

        public async Task HandleAsync(TriggerFollowUpMessagesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;

            await TriggerPhaseAsync(command.Message);

            channel.BasicAck(args.DeliveryTag, false);
        }

        protected override async Task TriggerPhaseAsync(TriggerPhaseMessageBodyBase message)
        {
            TriggerFollowUpMessageBody messageBody = message as TriggerFollowUpMessageBody;
            _logger.LogInformation("[TriggerFollowUpMessagesAsync] Exuecting TriggerFollowUpMessagesCommand for halId: {0}", messageBody.HalId);
            IList<PublishMessageBody> mqMessages = await _provider.CreateMQFollowUpMessagesAsync(messageBody.HalId);

            foreach (PublishMessageBody mqMessage in mqMessages)
            {
                await PublishMessageAsync(mqMessage);
            }
        }

        protected override async Task PublishMessageAsync(PublishMessageBody message)
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
