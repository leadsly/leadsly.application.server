using Leadsly.Application.Model;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators
{
    public class NetworkingMQCreator : MQCreatorBase, INetworkingMQCreator
    {
        public NetworkingMQCreator(
            ILogger<NetworkingMQCreator> logger,
            IHangfireService hangfireService,
            ITimestampService timestampService,
            IMessageBrokerOutlet messageBrokerOutlet,
            IWebHostEnvironment env,
            INetworkingCreateMQService service)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _env = env;
            _logger = logger;
            _hangfireService = hangfireService;
            _timestampService = timestampService;
            _service = service;
        }

        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<NetworkingMQCreator> _logger;
        private readonly IHangfireService _hangfireService;
        private readonly ITimestampService _timestampService;
        private readonly INetworkingCreateMQService _service;

        public async Task PublishMessageAsync(string halId, CancellationToken ct = default)
        {
            IList<PublishMessageBody> mqMessages = await _service.CreateMQNetworkingMessagesAsync(halId, ct);
            await PublishMessagesAsync(mqMessages);
        }

        public async Task PublishMessageAsync(string halId, Campaign campaign, CancellationToken ct = default)
        {
            IList<PublishMessageBody> mqMessages = await _service.CreateMQNetworkingMessagesAsync(halId, campaign, ct);
            await PublishMessagesAsync(mqMessages);
        }

        private async Task PublishMessagesAsync(IList<PublishMessageBody> mqMessages)
        {
            string halId = string.Empty;
            if (mqMessages != null)
            {
                foreach (PublishMessageBody mqMessage in mqMessages)
                {
                    halId = mqMessage.HalId;
                    _logger.LogInformation($"Publishing {nameof(NetworkingMessageBody)} MQ message for HalId {halId}", halId);
                    ((NetworkingMessageBody)mqMessage).NowLocalized = await _timestampService.GetNowLocalizedAsync(halId);
                    PublishMessage(mqMessage);
                }
            }
            else
            {
                _logger.LogDebug($"{nameof(NetworkingMessageBody)} MQ messages were null. Nothing will be published for HalId {halId}", halId);
            }
        }

        protected override void PublishMessage(PublishMessageBody message)
        {
            string queueNameIn = RabbitMQConstants.Networking.QueueName;
            string routingKeyIn = RabbitMQConstants.Networking.RoutingKey;
            string halId = message.HalId;
            NetworkingMessageBody messageBody = message as NetworkingMessageBody;

            DateTimeOffset nowLocalized = messageBody.NowLocalized;
            DateTimeOffset phaseStartDateTimeOffset = _timestampService.ParseDateTimeOffsetLocalized(message.TimeZoneId, messageBody.StartTime);

            if (nowLocalized.TimeOfDay < phaseStartDateTimeOffset.TimeOfDay)
            {
                _logger.LogInformation($"[Networking] This phase will not be triggered today because it is in the past {phaseStartDateTimeOffset}. Current local time is: {nowLocalized}. HalId {halId}");
                if (_env.IsDevelopment())
                {
                    // temporary to schedule jobs right away                
                    _logger.LogTrace("Development env detected. Executing phase immediately");
                    _messageBrokerOutlet.PublishPhase(message, queueNameIn, routingKeyIn, halId, null);
                }
                else
                {
                    _logger.LogInformation($"[Networking] This phase will be scheduled to start at {phaseStartDateTimeOffset}. Current local time is: {nowLocalized}. HalId {halId}");
                    _hangfireService.Schedule<IMessageBrokerOutlet>(x => x.PublishPhase(message, queueNameIn, routingKeyIn, halId, null), phaseStartDateTimeOffset);
                }
            }
        }
    }
}
