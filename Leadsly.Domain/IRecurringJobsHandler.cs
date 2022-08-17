using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface IRecurringJobsHandler
    {
        public Task PublishJobsAsync(string timeZoneId);

        public Task PublishRestartJobsPerTimezoneAsync(string timeZoneId);

        public Task ScheduleJobsForNewTimeZonesAsync();

        public Task ScheduleRestartJobsForNewTimeZonesAsync();
    }
}
