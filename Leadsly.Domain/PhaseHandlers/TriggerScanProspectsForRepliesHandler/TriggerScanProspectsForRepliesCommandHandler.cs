using Leadsly.Domain.Models.RabbitMQMessages;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers.TriggerScanProspectsForRepliesHandler
{
    public class TriggerScanProspectsForRepliesCommandHandler : ICommandHandler<TriggerScanProspectsForRepliesCommand>
    {
        public TriggerScanProspectsForRepliesCommandHandler(
            ILogger<TriggerScanProspectsForRepliesCommandHandler> logger,
            IScanProspectsForRepliesMQCreator mqCreator)
        {
            _logger = logger;
            _mqCreator = mqCreator;
        }

        private readonly IScanProspectsForRepliesMQCreator _mqCreator;
        private readonly ILogger<TriggerScanProspectsForRepliesCommandHandler> _logger;

        public async Task HandleAsync(TriggerScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;

            TriggerScanProspectsForRepliesMessageBody message = command.Message as TriggerScanProspectsForRepliesMessageBody;
            await _mqCreator.PublishMessageAsync(message.HalId);

            _logger.LogInformation($"Positively acknowledging {nameof(TriggerScanProspectsForRepliesMessageBody)}");
            channel.BasicAck(args.DeliveryTag, false);
        }

    }
}
