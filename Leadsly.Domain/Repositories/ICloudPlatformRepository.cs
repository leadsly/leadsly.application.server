using Leadsly.Application.Model.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ICloudPlatformRepository
    {
        public CloudPlatformConfiguration GetCloudPlatformConfiguration();
        public Task<EcsTaskDefinition> AddEcsTaskDefinitionAsync(EcsTaskDefinition newEcsTaskDefinition, CancellationToken ct = default);
        public Task<EcsService> AddEcsServiceAsync(EcsService newEcsService, CancellationToken ct = default);
        public Task<bool> RemoveEcsTaskDefinitionAsync(string ecsTaskDefinitionId, CancellationToken ct = default);
        public Task<bool> RemoveEcsServiceAsync(string ecsServiceId, CancellationToken ct = default);
        public Task<bool> RemoveCloudMapServiceDiscoveryServiceAsync(string discoveryServiceId, CancellationToken ct = default);
        public Task<CloudMapDiscoveryService> AddServiceDiscoveryAsync(CloudMapDiscoveryService newCloudMapServiceDiscovery, CancellationToken ct = default);
        public Task<VirtualAssistant> CreateVirtualAssistantAsync(VirtualAssistant newVirtualAssistant, CancellationToken ct = default);
        public Task<IList<VirtualAssistant>> GetAllVirtualAssistantByUserIdAsync(string userId, CancellationToken ct = default);
    }
}
