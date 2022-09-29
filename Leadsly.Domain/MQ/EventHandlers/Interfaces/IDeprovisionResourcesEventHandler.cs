using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.EventHandlers.Interfaces
{
    public interface IDeprovisionResourcesEventHandler
    {
        public Task OnDeprovisionResourcesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
