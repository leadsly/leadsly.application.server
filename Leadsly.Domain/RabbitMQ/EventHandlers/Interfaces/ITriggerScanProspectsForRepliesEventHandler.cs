using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.RabbitMQ.EventHandlers.Interfaces
{
    public interface ITriggerScanProspectsForRepliesEventHandler
    {
        public Task OnTriggerScanProspectsForRepliesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
