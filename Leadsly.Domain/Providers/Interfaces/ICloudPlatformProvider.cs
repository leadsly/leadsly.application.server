using Leadsly.Domain.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICloudPlatformProvider
    {
        Task<VirtualAssistant> GetVirtualAssistantAsync(string userId, CancellationToken ct = default);
        Task<VirtualAssistant> CreateVirtualAssistantAsync(EcsTaskDefinition newEcsTaskDefinition, EcsService newEcsService, CloudMapDiscoveryService newService, string halId, string userId, string timezoneId, CancellationToken ct = default);
        Task<bool> DeleteVirtualAssistantAsync(string virtualAssistantId, CancellationToken ct = default);
        Task<EcsTaskDefinition> RegisterTaskDefinitionInAwsAsync(string halId, CancellationToken ct = default);
        Task<CloudMapDiscoveryService> CreateCloudMapDiscoveryServiceInAwsAsync(CancellationToken ct = default);
        Task<EcsService> CreateEcsServiceInAwsAsync(string taskDefinition, string cloudMapServiceArn, CancellationToken ct = default);
        public Task<bool> EnsureEcsServiceTasksAreRunningAsync(string ecsServiceName, string clusterArn, CancellationToken ct = default);
        Task DeleteAwsEcsServiceAsync(string userId, string serviceName, string clusterName, CancellationToken ct = default);
        Task DeleteAwsCloudMapServiceAsync(string userId, string serviceDiscoveryId, CancellationToken ct = default);
        Task DeleteAwsTaskDefinitionRegistrationAsync(string userId, string taskDefinitionFamily, CancellationToken ct = default);
        Task DeleteEcsServiceAsync(string ecsServiceId, CancellationToken ct = default);
        Task DeleteCloudMapServiceAsync(string cloudMapServiceId, CancellationToken ct = default);
        Task DeleteTaskDefinitionRegistrationAsync(string taskDefinitionId, CancellationToken ct = default);
    }
}
