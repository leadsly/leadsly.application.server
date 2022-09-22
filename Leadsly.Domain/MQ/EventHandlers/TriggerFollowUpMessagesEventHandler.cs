using Leadsly.Domain.Models.RabbitMQMessages;
using Leadsly.Domain.MQ.EventHandlers.Interfaces;
using Leadsly.Domain.PhaseHandlers;
using Leadsly.Domain.PhaseHandlers.TriggerFollowUpMessagesHandler;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.EventHandlers
{
    public class TriggerFollowUpMessagesEventHandler : PhaseEventHandlerBase, ITriggerFollowUpMessagesEventHandler
    {
        public TriggerFollowUpMessagesEventHandler(ICommandHandler<TriggerFollowUpMessagesCommand> triggerFollowUpHandler, ILogger<TriggerFollowUpMessagesEventHandler> logger)
            : base(logger)
        {
            _triggerFollowUpHandler = triggerFollowUpHandler;
            _logger = logger;
        }

        private readonly ILogger<TriggerFollowUpMessagesEventHandler> _logger;
        private readonly ICommandHandler<TriggerFollowUpMessagesCommand> _triggerFollowUpHandler;

        public async Task OnTriggerFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            TriggerPhaseMessageBodyBase followUpMessage = DeserializeMessage<TriggerPhaseMessageBodyBase>(message);

            TriggerFollowUpMessagesCommand followUpMessageCommand = new TriggerFollowUpMessagesCommand(channel, eventArgs, followUpMessage);
            await _triggerFollowUpHandler.HandleAsync(followUpMessageCommand);
        }
    }
}
