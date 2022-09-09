using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.MQ.Creators.Interfaces;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class MonitorForNewConnectionsJobService : IMonitorForNewConnectionsJobService
    {
        public MonitorForNewConnectionsJobService(IMonitorForNewConnectionsMQCreator creator)
        {
            _creator = creator;
        }

        private readonly IMonitorForNewConnectionsMQCreator _creator;

        public async Task PublishMonitorForNewConnectionsMQMessagesAsync(string halId)
        {
            await _creator.PublishMessageAsync(halId);
        }
    }
}
