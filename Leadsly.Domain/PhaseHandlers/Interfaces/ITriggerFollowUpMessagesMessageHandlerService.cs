using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseHandlers.Interfaces
{
    public interface ITriggerFollowUpMessagesMessageHandlerService
    {
        public Task OnTriggerFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
