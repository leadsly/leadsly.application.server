using Leadsly.Application.Model;
using Leadsly.Domain.MQ.EventHandlers.Interfaces;
using Leadsly.Domain.MQ.Interfaces;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseConsumers.DeprovisionResourcesHandler
{
    public class DeprovisionResourcesConsumeCommandHandler : IConsumeCommandHandler<DeprovisionResourcesConsumeCommand>
    {
        public DeprovisionResourcesConsumeCommandHandler(IRabbitMQManager rabbitMQManager, IDeprovisionResourcesEventHandler eventHandlerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _eventHandlerService = eventHandlerService;
        }

        private readonly IDeprovisionResourcesEventHandler _eventHandlerService;
        private readonly IRabbitMQManager _rabbitMQManager;
        public Task ConsumeAsync(DeprovisionResourcesConsumeCommand command)
        {
            string queueNameIn = RabbitMQConstants.DeprovisionResources.QueueName;
            string routingKeyIn = RabbitMQConstants.DeprovisionResources.RoutingKey;

            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _eventHandlerService.OnTriggerFollowUpMessageEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
