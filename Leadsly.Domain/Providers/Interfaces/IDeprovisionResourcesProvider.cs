using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface IDeprovisionResourcesProvider
    {
        Task DeprovisionResourcesAsync(string halId, CancellationToken ct = default);
    }
}
