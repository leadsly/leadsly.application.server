using Leadsly.Domain.MQ.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Leadsly.Domain.PhaseHandlers.DeprovisionResourcesHandler
{
    public class DeprovisionResourcesCommand : ICommand
    {
        public DeprovisionResourcesCommand(IModel channel, BasicDeliverEventArgs eventArgs, DeprovisionResourcesBody message)
        {
            Channel = channel;
            EventArgs = eventArgs;
            Message = message;
        }

        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public DeprovisionResourcesBody Message { get; set; }
    }
}
