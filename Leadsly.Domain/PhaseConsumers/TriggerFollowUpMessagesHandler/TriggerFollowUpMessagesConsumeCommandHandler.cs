using Leadsly.Application.Model;
using Leadsly.Domain.RabbitMQ.EventHandlers.Interfaces;
using Leadsly.Domain.RabbitMQ.Interfaces;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseConsumers.TriggerFollowUpMessagesHandler
{
    public class TriggerFollowUpMessagesConsumeCommandHandler : IConsumeCommandHandler<TriggerFollowUpMessagesConsumeCommand>
    {
        public TriggerFollowUpMessagesConsumeCommandHandler(IRabbitMQManager rabbitMQManager, ITriggerFollowUpMessagesEventHandler eventHandlerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _eventHandlerService = eventHandlerService;
        }

        private readonly ITriggerFollowUpMessagesEventHandler _eventHandlerService;
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
