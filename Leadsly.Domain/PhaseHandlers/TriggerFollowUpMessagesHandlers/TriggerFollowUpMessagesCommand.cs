using Leadsly.Domain.Models.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Leadsly.Domain.PhaseHandlers.TriggerFollowUpMessagesHandlers
{
    public class TriggerFollowUpMessagesCommand : ICommand
    {
        public TriggerFollowUpMessagesCommand(IModel channel, BasicDeliverEventArgs eventArgs, TriggerFollowUpMessageBody message)
        {
            Channel = channel;
            EventArgs = eventArgs;
            Message = message;
        }

        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public TriggerFollowUpMessageBody Message { get; private set; }
    }
}
