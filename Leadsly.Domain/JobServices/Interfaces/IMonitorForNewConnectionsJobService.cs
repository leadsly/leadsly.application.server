using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface IMonitorForNewConnectionsJobService
    {
        public Task PublishMonitorForNewConnectionsMQMessagesAsync(string halId);
    }
}
