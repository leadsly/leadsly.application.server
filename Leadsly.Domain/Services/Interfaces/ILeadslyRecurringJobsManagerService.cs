using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ILeadslyRecurringJobsManagerService
    {
        public Task PublishMessagesAsync(string halId);
    }
}
