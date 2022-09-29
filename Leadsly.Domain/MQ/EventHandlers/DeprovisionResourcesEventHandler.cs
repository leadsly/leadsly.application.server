using Leadsly.Domain.MQ.EventHandlers.Interfaces;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.PhaseHandlers;
using Leadsly.Domain.PhaseHandlers.DeprovisionResourcesHandler;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.EventHandlers
{
    public class DeprovisionResourcesEventHandler : PhaseEventHandlerBase, IDeprovisionResourcesEventHandler
    {
        public DeprovisionResourcesEventHandler(
            ICommandHandler<DeprovisionResourcesCommand> deprovisionResourcesHandler,
            ILogger<DeprovisionResourcesEventHandler> logger)
            : base(logger)
        {
            _deprovisionResourcesHandler = deprovisionResourcesHandler;
            _logger = logger;
        }

        private readonly ILogger<DeprovisionResourcesEventHandler> _logger;
        private readonly ICommandHandler<DeprovisionResourcesCommand> _deprovisionResourcesHandler;

        public async Task OnDeprovisionResourcesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            DeprovisionResourcesBody deprovisionResourcesMessage = DeserializeMessage<DeprovisionResourcesBody>(message);

            DeprovisionResourcesCommand deprovisionResourcesCommand = new DeprovisionResourcesCommand(channel, eventArgs, deprovisionResourcesMessage);

            await _deprovisionResourcesHandler.HandleAsync(deprovisionResourcesCommand);
        }

    }
}
