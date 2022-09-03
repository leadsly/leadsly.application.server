using Leadsly.Application.Model;
using Leadsly.Domain.PhaseHandlers.Interfaces;
using Leadsly.Domain.RabbitMQ;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseConsumers.TriggerFollowUpMessagesHandler
{
    public class TriggerFollowUpMessagesConsumerCommandHandler : IConsumeCommandHandler<TriggerFollowUpMessagesConsumeCommand>
    {
        public TriggerFollowUpMessagesConsumerCommandHandler(IRabbitMQManager rabbitMQManager, ITriggerFollowUpMessagesMessageHandlerService eventHandlerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _eventHandlerService = eventHandlerService;
        }

        private readonly ITriggerFollowUpMessagesMessageHandlerService _eventHandlerService;
        private readonly IRabbitMQManager _rabbitMQManager;
        public Task ConsumeAsync(TriggerFollowUpMessagesConsumeCommand command)
        {
            string queueNameIn = RabbitMQConstants.TriggerFollowUpMessages.QueueName;
            string routingKeyIn = RabbitMQConstants.TriggerFollowUpMessages.RoutingKey;

            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _eventHandlerService.OnTriggerFollowUpMessageEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
