using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface IAllInOneVirtualAssistantJobService
    {
        public Task PublishAllInOneVirtualAssistantPhaseAsync(string halId, bool initialPhase);
    }
}
