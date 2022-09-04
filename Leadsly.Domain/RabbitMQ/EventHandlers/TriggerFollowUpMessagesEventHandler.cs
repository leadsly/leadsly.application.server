using Leadsly.Domain.Models.RabbitMQMessages;
using Leadsly.Domain.PhaseConsumers;
using Leadsly.Domain.PhaseHandlers.TriggerFollowUpMessagesHandler;
using Leadsly.Domain.RabbitMQ.EventHandlers.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.RabbitMQ.EventHandlers
{
    public class TriggerFollowUpMessagesEventHandler : TriggerPhaseEventHandlerBase, ITriggerFollowUpMessagesEventHandler
    {
        public TriggerFollowUpMessagesEventHandler(ICommandHandler<TriggerFollowUpMessagesCommand> triggerFollowUpHandler, ILogger<TriggerFollowUpMessagesEventHandler> logger)
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
            TriggerPhaseMessageBodyBase followUpMessage = DeserializeMessage(message);

            TriggerFollowUpMessagesCommand followUpMessageCommand = new TriggerFollowUpMessagesCommand(channel, eventArgs, followUpMessage);
            await _triggerFollowUpHandler.HandleAsync(followUpMessageCommand);
        }

        protected override TriggerPhaseMessageBodyBase DeserializeMessage(string rawMessage)
        {
            _logger.LogInformation("Deserializing TriggerFollowUpMessageBody");
            TriggerFollowUpMessageBody triggerFollowUpMessageBody = null;
            try
            {
                triggerFollowUpMessageBody = JsonConvert.DeserializeObject<TriggerFollowUpMessageBody>(rawMessage);
                _logger.LogDebug("Successfully deserialized TriggerFollowUpMessageBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize TriggerFollowUpMessageBody. Returning an explicit null");
                return null;
            }

            return triggerFollowUpMessageBody;
        }
    }
}
