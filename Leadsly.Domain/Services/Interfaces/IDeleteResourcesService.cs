using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IDeleteResourcesService
    {
        public Task<bool> DeleteEcsServiceAsync(string ecsServiceId, CancellationToken ct = default);
        public Task<bool> DeleteCloudMapServiceAsync(string cloudMapServiceId, CancellationToken ct = default);
    }
}
