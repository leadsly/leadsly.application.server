using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ILeadslyRecurringJobsManagerService
    {
        public Task RestartHalAsync(string halId);
        public Task PublishHalPhasesAsync(string halId);
    }
}
