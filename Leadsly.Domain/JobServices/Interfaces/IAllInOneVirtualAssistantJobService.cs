using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface IAllInOneVirtualAssistantJobService
    {
        /// <summary>
        /// Execution hour parameter is simply used as a unique identifier for the job
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="initialPhase"></param>
        /// <param name="executionHour"></param>
        /// <returns></returns>
        [ExecuteOncePhasesOnce]
        public Task PublishAllInOneVirtualAssistantPhaseAsync(string halId, bool initialPhase, string executionHour);
    }
}
