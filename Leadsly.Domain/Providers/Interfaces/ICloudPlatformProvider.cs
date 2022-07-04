using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICloudPlatformProvider
    {
        Task<bool> RemoveCloudResourcesAsync(string ecsServiceId, string ecsTaskDefinitionId, string cloudMapDiscoveryServiceId, CancellationToken ct = default);
        Task<VirtualAssistant> GetVirtualAssistantAsync(string userId, CancellationToken ct = default);
        Task<VirtualAssistant> CreateVirtualAssistantAsync(EcsTaskDefinition newEcsTaskDefinition, EcsService newEcsService, CloudMapDiscoveryService newService, string halId, string userId, string timezoneId, CancellationToken ct = default);
        Task<bool> DeleteVirtualAssistantAsync(string virtualAssistantId, CancellationToken ct = default);
        public Task<NewSocialAccountSetupResult> SetupNewCloudResourceForUserSocialAccountAsync(string userId, CancellationToken ct = default);
        Task<EcsTaskDefinitionDTO> RegisterTaskDefinitionInAwsAsync(string halId, LeadslyAccountSetupResult result, CancellationToken ct = default);
        Task<CloudMapDiscoveryServiceDTO> CreateCloudMapDiscoveryServiceInAwsAsync(LeadslyAccountSetupResult result, CancellationToken ct = default);
        Task<EcsServiceDTO> CreateEcsServiceInAwsAsync(string taskDefinition, string cloudMapServiceArn, LeadslyAccountSetupResult result, CancellationToken ct = default);
        public Task<bool> AreAwsResourcesHealthyAsync(string cloudMapServiceName, CancellationToken ct = default);
        public Task<bool> EnsureEcsServiceTasksAreRunningAsync(string ecsServiceName, string clusterArn, CancellationToken ct = default);
        public Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, string userId, CancellationToken ct = default);
        Task DeleteAwsEcsServiceAsync(string userId, string serviceName, string clusterName, CancellationToken ct = default);
        Task DeleteAwsCloudMapServiceAsync(string userId, string serviceDiscoveryId, CancellationToken ct = default);
        Task DeleteAwsTaskDefinitionRegistrationAsync(string userId, string taskDefinitionFamily, CancellationToken ct = default);
        Task DeleteEcsServiceAsync(string ecsServiceId, CancellationToken ct = default);
        Task DeleteCloudMapServiceAsync(string cloudMapServiceId, CancellationToken ct = default);
        Task DeleteTaskDefinitionRegistrationAsync(string taskDefinitionId, CancellationToken ct = default);
        public Task<ExistingSocialAccountSetupResultDTO> ConnectToExistingCloudResourceAsync(SocialAccount socialAccount, CancellationToken ct = default);
        public Task RemoveUsersSocialAccountCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default);
    }
}
