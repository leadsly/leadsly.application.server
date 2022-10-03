using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface INetworkingJobsService
    {
        // [DisableMultipleQueuedItemsFilter]
        public Task PublishNetworkingMQMessagesAsync(string halId);
    }
}
