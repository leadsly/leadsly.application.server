using Leadsly.Domain.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IDeprovisionResourcesService
    {
        public Task<bool> DeleteEcsServiceAsync(EcsService serviceToRemove, CancellationToken ct = default);
    }
}
