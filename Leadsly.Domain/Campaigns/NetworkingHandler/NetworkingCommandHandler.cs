using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Campaigns.Handlers;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.NetworkingHandler
{
    public class NetworkingCommandHandler : ICommandHandler<NetworkingCommand>
    {
        public NetworkingCommandHandler(
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<NetworkingCommandHandler> logger,
            ITimestampService timestampService,
            IHangfireService hangfireService,
            INetworkingMessagesFactory networkingMessagesFactory
            )
        {
            _hangfireService = hangfireService;
            _networkingMessagesFactory = networkingMessagesFactory;
            _timestampService = timestampService;
            _messageBrokerOutlet = messageBrokerOutlet;
            _logger = logger;
        }

        private readonly IHangfireService _hangfireService;
        private readonly INetworkingMessagesFactory _networkingMessagesFactory;
        private readonly ITimestampService _timestampService;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<NetworkingCommandHandler> _logger;

        public async Task HandleAsync(NetworkingCommand command)
        {
            if (command.HalIds != null && command.HalIds.Count > 0)
            {
                await HandleInternalAsync(command.HalIds);
            }
            else if (string.IsNullOrEmpty(command.HalId) == false)
            {
                await HandleInternalAsync(command.HalId);
            }
            // triggered on new campaign
            else if (command.CampaignId != null && command.UserId != null)
            {
                await HandleInternalAsync(command.CampaignId, command.UserId);
            }
        }

        private async Task HandleInternalAsync(IList<string> halIds, CancellationToken ct = default)
        {
            foreach (string halId in halIds)
            {
                await HandleInternalAsync(halId, ct);
            }
        }

        private async Task HandleInternalAsync(string halId, CancellationToken ct = default)
        {
            IList<NetworkingMessageBody> messages = await _networkingMessagesFactory.CreateNetworkingMessagesAsync(halId, ct);

            await SchedulePhaseMessagesAsync(messages);
        }

        private async Task HandleInternalAsync(string campaignId, string userId, CancellationToken ct = default)
        {
            IList<NetworkingMessageBody> messages = await _networkingMessagesFactory.CreateNetworkingMessagesAsync(campaignId, userId, ct);
            await SchedulePhaseMessagesAsync(messages);
        }

        private async Task SchedulePhaseMessagesAsync(IList<NetworkingMessageBody> messages)
        {
            foreach (NetworkingMessageBody message in messages)
            {
                await SchedulePhaseMessageAsync(message);
            }
        }

        private async Task SchedulePhaseMessageAsync(NetworkingMessageBody message)
        {
            string queueNameIn = RabbitMQConstants.Networking.QueueName;
            string routingKeyIn = RabbitMQConstants.Networking.RoutingKey;
            string halId = message.HalId;

            DateTimeOffset nowLocalized = await _timestampService.GetNowLocalizedAsync(halId);
            DateTimeOffset phaseStartDateTimeOffset = _timestampService.ParseDateTimeOffsetLocalized(message.TimeZoneId, message.StartTime);

            if (nowLocalized.TimeOfDay < phaseStartDateTimeOffset.TimeOfDay)
            {
                _logger.LogInformation($"[Networking] This phase will be scheduled to start at {phaseStartDateTimeOffset}. Current local time is: {nowLocalized}");
                _hangfireService.Schedule<IMessageBrokerOutlet>(x => x.PublishPhase(message, queueNameIn, routingKeyIn, halId, null), phaseStartDateTimeOffset);                
            }
            else
            {
                _logger.LogInformation($"[Networking] This phase will not be triggered today because it is in the past {phaseStartDateTimeOffset}. Current local time is: {nowLocalized}");
                // temporary to schedule jobs right away                
                // _messageBrokerOutlet.PublishPhase(message, queueNameIn, routingKeyIn, halId, null);
            }
        }
    }
}
