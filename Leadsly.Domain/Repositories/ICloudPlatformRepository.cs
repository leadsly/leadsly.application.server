using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Application.Model;

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
        public Task<CloudMapServiceDiscoveryService> AddServiceDiscoveryAsync(CloudMapServiceDiscoveryService newCloudMapServiceDiscovery, CancellationToken ct = default);
    }
}
