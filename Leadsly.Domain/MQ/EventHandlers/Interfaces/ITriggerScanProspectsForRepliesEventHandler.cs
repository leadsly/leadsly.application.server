using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.EventHandlers.Interfaces
{
    public interface ITriggerScanProspectsForRepliesEventHandler
    {
        public Task OnTriggerScanProspectsForRepliesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
