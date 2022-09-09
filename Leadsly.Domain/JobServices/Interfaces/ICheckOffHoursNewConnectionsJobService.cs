using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface ICheckOffHoursNewConnectionsJobService
    {
        public Task PublishCheckOffHoursNewConnectionsMQMessageAsync(string halId);
    }
}
