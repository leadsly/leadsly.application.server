using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface IRecurringJobsHandler
    {
        // public Task CreateAndPublishJobsAsync();
        public Task PublishJobsAsync(string timeZoneId);

        public Task ScheduleJobsForNewTimeZonesAsync();
    }
}
