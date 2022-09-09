using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface IRestartHalsByTimezoneJobService
    {
        public Task PublishRestartJobsPerTimezoneAsync(string timeZoneId);
    }
}
