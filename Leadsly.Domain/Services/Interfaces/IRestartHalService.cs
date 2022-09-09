using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IRestartHalService
    {
        public Task RestartAsync(string halId, CancellationToken ct = default);
    }
}
