using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.MQ.Creators.Interfaces;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class CheckOffHoursNewConnectionsJobService : ICheckOffHoursNewConnectionsJobService
    {
        public CheckOffHoursNewConnectionsJobService(ICheckOffHoursNewConnectionsMQCreator creator)
        {
            _creator = creator;
        }

        private readonly ICheckOffHoursNewConnectionsMQCreator _creator;

        public async Task PublishCheckOffHoursNewConnectionsMQMessageAsync(string halId)
        {
            await _creator.PublishMessageAsync(halId);
        }
    }
}
