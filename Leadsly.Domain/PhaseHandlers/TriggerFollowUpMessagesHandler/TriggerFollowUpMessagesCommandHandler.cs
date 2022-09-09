using Leadsly.Domain.Models.RabbitMQMessages;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers.TriggerFollowUpMessagesHandler
{
    public class TriggerFollowUpMessagesCommandHandler : ICommandHandler<TriggerFollowUpMessagesCommand>
    {
        public TriggerFollowUpMessagesCommandHandler(
            IFollowUpMessagesMQCreator mqCreator,
            ILogger<TriggerFollowUpMessagesCommandHandler> logger)
        {
            _mqCreator = mqCreator;
            _logger = logger;
        }

        private readonly ILogger<TriggerFollowUpMessagesCommandHandler> _logger;
        private readonly IFollowUpMessagesMQCreator _mqCreator;

        public async Task HandleAsync(TriggerFollowUpMessagesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;

            TriggerFollowUpMessageBody message = command.Message as TriggerFollowUpMessageBody;
            await _mqCreator.PublishMessageAsync(message.HalId);

            _logger.LogInformation($"Positively acknowledging {nameof(TriggerFollowUpMessageBody)}");
            channel.BasicAck(args.DeliveryTag, false);
        }

    }
}
