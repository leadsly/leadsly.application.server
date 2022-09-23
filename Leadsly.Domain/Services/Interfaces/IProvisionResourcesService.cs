using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IProvisionResourcesService
    {
        public IList<EcsService> EcsServices { get; }
        public IList<EcsTaskDefinition> EcsTaskDefinitions { get; }
        public IList<CloudMapDiscoveryService> CloudMapDiscoveryServices { get; }
        Task<bool> CreateAwsResourcesAsync(string halId, string userId, CancellationToken ct = default);
        Task<bool> CreateAwsTaskDefinitionsAsync(string halId, string userId, CancellationToken ct = default);
        Task RollbackAllResourcesAsync(string userId, CancellationToken ct = default);
    }
}
