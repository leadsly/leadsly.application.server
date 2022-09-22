using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IDeprovisionResourcesService
    {
        public Task<bool> DeleteEcsServiceAsync(EcsService serviceToRemove, CancellationToken ct = default);
        public Task<bool> StopAllEcsTasksAsync(IEnumerable<EcsTask> ecsTasks, string cluster, EcsResourcePurpose purpose, CancellationToken ct = default);
        public Task<bool> DeleteCloudMapServiceAsync(string serviceDiscoveryId, CancellationToken ct = default);
    }
}
