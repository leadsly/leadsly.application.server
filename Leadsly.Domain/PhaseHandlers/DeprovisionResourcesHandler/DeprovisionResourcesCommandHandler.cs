using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers.DeprovisionResourcesHandler
{
    public class DeprovisionResourcesCommandHandler : ICommandHandler<DeprovisionResourcesCommand>
    {
        public DeprovisionResourcesCommandHandler(
            ILogger<DeprovisionResourcesCommandHandler> logger,
            IDeprovisionResourcesProvider provider)
        {
            _provider = provider;
            _logger = logger;
        }

        private readonly IDeprovisionResourcesProvider _provider;
        private readonly ILogger<DeprovisionResourcesCommandHandler> _logger;

        public async Task HandleAsync(DeprovisionResourcesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;

            DeprovisionResourcesBody message = command.Message;
            await _provider.DeprovisionResourcesAsync(message.HalId);

            _logger.LogInformation($"Positively acknowledging {nameof(DeprovisionResourcesBody)}");
            channel.BasicAck(args.DeliveryTag, false);
        }
    }

}
