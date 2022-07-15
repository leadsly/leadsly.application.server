using Leadsly.Domain.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IOrphanedCloudResourcesRepository
    {
        Task<OrphanedCloudResource> AddOrphanedCloudResourceAsync(OrphanedCloudResource orphanedResource, CancellationToken ct = default);
    }
}
