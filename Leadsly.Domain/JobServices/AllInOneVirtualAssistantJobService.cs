using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.MQ.Creators.Interfaces;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class AllInOneVirtualAssistantJobService : IAllInOneVirtualAssistantJobService
    {
        public AllInOneVirtualAssistantJobService(
            IAllInOneVirtualAssistantMQCreator mqCreator)
        {
            _mqCreator = mqCreator;
        }

        private readonly IAllInOneVirtualAssistantMQCreator _mqCreator;

        public async Task PublishAllInOneVirtualAssistantPhaseAsync(string halId, bool initial)
        {
            await _mqCreator.PublishMessageAsync(halId, initial);
        }
    }
}