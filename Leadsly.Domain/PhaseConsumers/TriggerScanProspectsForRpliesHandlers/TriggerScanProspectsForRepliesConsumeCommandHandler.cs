﻿using Leadsly.Application.Model;
using Leadsly.Domain.MQ.EventHandlers.Interfaces;
using Leadsly.Domain.MQ.Interfaces;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseConsumers.TriggerScanProspectsForRpliesHandlers
{
    public class TriggerScanProspectsForRepliesConsumeCommandHandler : IConsumeCommandHandler<TriggerScanProspectsForRepliesConsumeCommand>
    {
        public TriggerScanProspectsForRepliesConsumeCommandHandler(IRabbitMQManager rabbitMQManager, ITriggerScanProspectsForRepliesEventHandler eventHandlerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _eventHandlerService = eventHandlerService;
        }

        private readonly ITriggerScanProspectsForRepliesEventHandler _eventHandlerService;
        private readonly IRabbitMQManager _rabbitMQManager;
        public Task ConsumeAsync(TriggerScanProspectsForRepliesConsumeCommand command)
        {
            string queueNameIn = RabbitMQConstants.TriggerScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.TriggerScanProspectsForReplies.RoutingKey;

            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _eventHandlerService.OnTriggerScanProspectsForRepliesEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
