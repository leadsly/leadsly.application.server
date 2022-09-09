using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface IProspectingJobService
    {
        public Task PublishProspectingMQMessagesAsync(string halId);
    }
}
