using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ILeadslyRecurringJobsManagerService
    {
        public Task RestartHalAsync(string halId);
        public Task PublishCheckOffHoursNewConnectionsAsync(string halId);
        public Task PublishMonitorForNewConnectionsAsync(string halId);
        public Task PublishProspectingPhaseAsync(string halId);
        public Task PublishNetworkingPhaseAsync(string halId);
    }
}
