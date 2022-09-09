using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices.Interfaces
{
    public interface IRestartHalJobService
    {
        public Task RestartHalAsync(string halId);
    }
}
