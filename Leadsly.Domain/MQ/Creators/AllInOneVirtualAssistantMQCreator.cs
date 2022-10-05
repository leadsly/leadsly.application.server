using Leadsly.Application.Model;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators
{
    public class AllInOneVirtualAssistantMQCreator : MQCreatorBase, IAllInOneVirtualAssistantMQCreator
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
            PublishMessageBody mqMessage = await _service.CreateMQAllInOneVirtualAssistantMessageAsync(halId, initial, ct);
            if (mqMessage == null)
            {
                _logger.LogError("Creating {0} did not complete successfully. The result is null. Perhaps there were stale jobs for HalId that was deleted that were executed?", nameof(AllInOneVirtualAssistantMQCreator));
                return;
            }
            string userId = mqMessage.UserId;
            if (string.IsNullOrEmpty(userId) == true)
            {
                throw new Exception("User Id must be set before continuing!");
            }

            // 2. provision hal resources            
            if (await _service.ProvisionResourcesAsync(halId, userId) == true)
            {
                _logger.LogInformation("Successfully created AWS resources, publishing {0}", nameof(AllInOneVirtualAssistantMessageBody));
                PublishMessage(mqMessage);
            }
            else
            {
                _logger.LogError("Failed to create AWS resources. No {0} will be published", nameof(AllInOneVirtualAssistantMessageBody));
            }
        }

        protected override void PublishMessage(PublishMessageBody message)
        {
            string halId = message.HalId;
            _logger.LogInformation("[HandleAsync] Exuecting {0} for halId: {1}", nameof(MonitorForNewAcceptedConnectionsBody), halId);
            string queueNameIn = RabbitMQConstants.AllInOneVirtualAssistant.QueueName;
            string routingKeyIn = RabbitMQConstants.AllInOneVirtualAssistant.RoutingKey;

            _messageBrokerOutlet.PublishPhase(message, queueNameIn, Guid.NewGuid().ToString(), routingKeyIn, halId, null);
        }
    }
}
