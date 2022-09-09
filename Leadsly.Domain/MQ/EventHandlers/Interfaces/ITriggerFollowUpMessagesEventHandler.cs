using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.EventHandlers.Interfaces
{
    public interface ITriggerFollowUpMessagesEventHandler
    {
        public Task OnTriggerFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
