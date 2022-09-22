using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface INewTimeZonesJobsService
    {
        public Task AddRecurringJobsForNewTimeZonesAsync();
        public Task AddRecurringJobsForNewTimeZonesAsync_AllInOneVirtualAssistant();
        public Task AddRecurringRestartJobsForNewTimeZonesAsync();
    }
}
