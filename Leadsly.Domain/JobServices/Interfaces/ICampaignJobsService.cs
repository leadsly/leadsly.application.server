using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface ICampaignJobsService
    {
        public Task ExecuteDailyJobsAsync(string timezoneId);
    }
}
