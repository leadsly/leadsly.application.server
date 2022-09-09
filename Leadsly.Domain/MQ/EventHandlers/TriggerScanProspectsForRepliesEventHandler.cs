using Leadsly.Domain.Models.RabbitMQMessages;
using Leadsly.Domain.MQ.EventHandlers.Interfaces;
using Leadsly.Domain.PhaseConsumers;
using Leadsly.Domain.PhaseHandlers;
using Leadsly.Domain.PhaseHandlers.TriggerScanProspectsForRepliesHandler;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.EventHandlers
{
    public class TriggerScanProspectsForRepliesEventHandler : TriggerPhaseEventHandlerBase, ITriggerScanProspectsForRepliesEventHandler
    {
        public TriggerScanProspectsForRepliesEventHandler(ICommandHandler<TriggerScanProspectsForRepliesCommand> triggerScanProspectsHandler, ILogger<TriggerScanProspectsForRepliesEventHandler> logger)
        {
            _triggerScanProspectsHandler = triggerScanProspectsHandler;
            _logger = logger;
        }

        private readonly ILogger<TriggerScanProspectsForRepliesEventHandler> _logger;
        private readonly ICommandHandler<TriggerScanProspectsForRepliesCommand> _triggerScanProspectsHandler;

        public async Task OnTriggerScanProspectsForRepliesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            TriggerPhaseMessageBodyBase scanProspectsFollowUpMessage = DeserializeMessage(message);

            TriggerScanProspectsForRepliesCommand triggerScanProspectsForRepliesCommand = new TriggerScanProspectsForRepliesCommand(channel, eventArgs, scanProspectsFollowUpMessage);
            await _triggerScanProspectsHandler.HandleAsync(triggerScanProspectsForRepliesCommand);
        }

        protected override TriggerPhaseMessageBodyBase DeserializeMessage(string rawMessage)
        {
            _logger.LogInformation("Deserializing TriggerFollowUpMessageBody");
            TriggerScanProspectsForRepliesMessageBody triggerScanProspectsForRepliesMessage = null;
            try
            {
                triggerScanProspectsForRepliesMessage = JsonConvert.DeserializeObject<TriggerScanProspectsForRepliesMessageBody>(rawMessage);
                _logger.LogDebug("Successfully deserialized TriggerFollowUpMessageBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize TriggerFollowUpMessageBody. Returning an explicit null");
                return null;
            }

            return triggerScanProspectsForRepliesMessage;
        }
    }
}
