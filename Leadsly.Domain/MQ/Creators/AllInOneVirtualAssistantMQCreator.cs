﻿using Leadsly.Application.Model;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators
{
    public class AllInOneVirtualAssistantMQCreator : IAllInOneVirtualAssistantMQCreator
    {
        public AllInOneVirtualAssistantMQCreator(
            ILogger<AllInOneVirtualAssistantMQCreator> logger,
            IMessageBrokerOutlet messageBrokerOutlet,
            IAllInOneVirtualAssistantMQService service
            )
        {
            _service = service;
            _logger = logger;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly IAllInOneVirtualAssistantMQService _service;
        private readonly ILogger<AllInOneVirtualAssistantMQCreator> _logger;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        public async Task PublishMessageAsync(string halId, bool initial, CancellationToken ct = default)
        {
            // 1. publish all in one virtual assistant
            AllInOneVirtualAssistantMessageBody mqMessage = await _service.CreateMQAllInOneVirtualAssistantMessageAsync(halId, ct);
            string userId = mqMessage.CheckOffHoursNewConnections.UserId;
            if (string.IsNullOrEmpty(userId) == true)
            {
                throw new Exception("User Id must be set before continuing!");
            }

            // 2. provision hal resources
            bool succeeded = await _service.ProvisionResourcesAsync(halId, userId);

            if (succeeded == true)
            {
                _logger.LogInformation("Successfully created AWS resources, publishing {0}", nameof(AllInOneVirtualAssistantMessageBody));
                PublishMessage(mqMessage, initial);
            }
            else
            {
                _logger.LogError("Failed to create AWS resources. No {0} will be published", nameof(AllInOneVirtualAssistantMessageBody));
            }
        }

        protected void PublishMessage(AllInOneVirtualAssistantMessageBody message, bool initial)
        {
            string halId = message.HalId;
            _logger.LogInformation("[HandleAsync] Exuecting {0} for halId: {1}", nameof(MonitorForNewAcceptedConnectionsBody), halId);
            string queueNameIn = RabbitMQConstants.AllInOneVirtualAssistant.QueueName;
            string routingKeyIn = RabbitMQConstants.AllInOneVirtualAssistant.RoutingKey;

            Dictionary<string, object> headers = new Dictionary<string, object>();
            string headerKey = RabbitMQConstants.AllInOneVirtualAssistant.ExecuteType;
            string headerValue = initial ? RabbitMQConstants.AllInOneVirtualAssistant.Initial : RabbitMQConstants.AllInOneVirtualAssistant.Regular;
            _logger.LogInformation($"Setting rabbitMQ headers. Header key {0} header value is {1}", headerKey, headerValue);
            headers.Add(headerKey, headerValue);

            _messageBrokerOutlet.PublishPhase(message, queueNameIn, routingKeyIn, halId, headers);
        }
    }
}
