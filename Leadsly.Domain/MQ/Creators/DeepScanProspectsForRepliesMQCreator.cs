﻿using Leadsly.Application.Model;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators
{
    public class DeepScanProspectsForRepliesMQCreator : MQCreatorBase, IDeepScanProspectsForRepliesMQCreator
    {
        public DeepScanProspectsForRepliesMQCreator(
            ILogger<DeepScanProspectsForRepliesMQCreator> logger,
            IMessageBrokerOutlet messageBrokerOutlet,
            IDeepScanProspectsForRepliesCreateMQService service,
            IUserRepository userRepository
            )
        {
            _service = service;
            _userRepository = userRepository;
            _logger = logger;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly IUserRepository _userRepository;
        private readonly IDeepScanProspectsForRepliesCreateMQService _service;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly ILogger<DeepScanProspectsForRepliesMQCreator> _logger;

        public async Task<PublishMessageBody> CreateMQMessageAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("[DeepScanProspectsForRepliesBody] Exuecting {0} for halId: {1}.", halId, nameof(DeepScanProspectsForRepliesBody));

            SocialAccount socialAccount = await _userRepository.GetSocialAccountByHalIdAsync(halId, ct);
            if (socialAccount == null)
            {
                _logger.LogWarning("Unable to locate SocialAccount associated with halId {halId}. Cannot proceed", halId);
                return null;
            }

            string userId = socialAccount.UserId;
            string scanProspectsForRepliesPhaseId = socialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId;
            PublishMessageBody mqMessage = await _service.CreateMQMessageAsync(userId, halId, scanProspectsForRepliesPhaseId, ct);

            return mqMessage;
        }

        public async Task PublishMessageAsync(string halId, CancellationToken ct = default)
        {
            PublishMessageBody mqMessage = await CreateMQMessageAsync(halId, ct);

            if (mqMessage != null)
            {
                _logger.LogInformation($"Publishing {nameof(DeepScanProspectsForRepliesBody)} MQ message for HalId {halId}", halId);
                PublishMessage(mqMessage);
            }
            else
            {
                _logger.LogDebug($"{nameof(DeepScanProspectsForRepliesBody)} MQ message was null. Nothing will be published for HalId {halId}", halId);
            }
        }

        protected override void PublishMessage(PublishMessageBody message)
        {
            // same as ScanProspectsForReplies
            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            // same as ScanProspectsForReplies
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = message.HalId;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan);

            _messageBrokerOutlet.PublishPhase(message, queueNameIn, Guid.NewGuid().ToString(), routingKeyIn, halId, headers);
        }
    }
}
