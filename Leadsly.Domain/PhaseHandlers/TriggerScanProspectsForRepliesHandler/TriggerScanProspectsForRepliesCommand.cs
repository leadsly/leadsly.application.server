using Leadsly.Domain.Models.RabbitMQMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Leadsly.Domain.PhaseHandlers.TriggerScanProspectsForRepliesHandler
{
    public class TriggerScanProspectsForRepliesCommand : ICommand
    {
        public TriggerScanProspectsForRepliesCommand(IModel channel, BasicDeliverEventArgs eventArgs, TriggerPhaseMessageBodyBase message)
        {
            Channel = channel;
            EventArgs = eventArgs;
            Message = message;
        }

        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public TriggerPhaseMessageBodyBase Message { get; private set; }
    }
}
