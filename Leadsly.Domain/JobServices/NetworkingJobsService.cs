using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.MQ.Creators.Interfaces;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class NetworkingJobsService : INetworkingJobsService
    {
        public NetworkingJobsService(INetworkingMQCreator creator)
        {
            _creator = creator;
        }

        private readonly INetworkingMQCreator _creator;

        public async Task PublishNetworkingMQMessagesAsync(string halId)
        {
            await _creator.PublishMessageAsync(halId);
        }
    }
}
