using Leadsly.Domain.Models.RabbitMQ;
using Leadsly.Domain.PhaseHandlers.Interfaces;
using Leadsly.Domain.PhaseHandlers.TriggerFollowUpMessagesHandlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers
{
    public class TriggerFollowUpMessagesMessageHandlerService : ITriggerFollowUpMessagesMessageHandlerService
    {
        public TriggerFollowUpMessagesMessageHandlerService(ICommandHandler<TriggerFollowUpMessagesCommand> triggerFollowUpHandler, ILogger<TriggerFollowUpMessagesMessageHandlerService> logger)
        {
            _triggerFollowUpHandler = triggerFollowUpHandler;
            _logger = logger;
        }

        private readonly ILogger<TriggerFollowUpMessagesMessageHandlerService> _logger;
        private readonly ICommandHandler<TriggerFollowUpMessagesCommand> _triggerFollowUpHandler;

        public async Task OnTriggerFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            TriggerFollowUpMessageBody followUpMessage = DeserializeFollowUpMessagesBody(message);

            TriggerFollowUpMessagesCommand followUpMessageCommand = new TriggerFollowUpMessagesCommand(channel, eventArgs, followUpMessage);
            await _triggerFollowUpHandler.HandleAsync(followUpMessageCommand);
        }

        private TriggerFollowUpMessageBody DeserializeFollowUpMessagesBody(string body)
        {
            _logger.LogInformation("Deserializing TriggerFollowUpMessageBody");
            TriggerFollowUpMessageBody triggerFollowUpMessageBody = null;
            try
            {
                triggerFollowUpMessageBody = JsonConvert.DeserializeObject<TriggerFollowUpMessageBody>(body);
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
