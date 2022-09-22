using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface ICampaignJobsService
    {
        public Task ExecuteDailyJobsAsync(string timezoneId);

        public Task ExecuteDailyJobsAsync_AllInOneVirtualAssistant(string timezoneId);

        public Task RunMarkProspectsAsCompleteAsyncDailyJobAsync(string timezoneId);
    }
}
