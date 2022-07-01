using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICloudPlatformProvider
    {
        Task<VirtualAssistant> GetVirtualAssistantAsync(string userId, CancellationToken ct = default);
        Task<VirtualAssistant> CreateVirtualAssistantAsync(EcsTaskDefinition newEcsTaskDefinition, EcsService newEcsService, CloudMapDiscoveryService newService, string halId, string userId, string timezoneId, CancellationToken ct = default);
        public Task<NewSocialAccountSetupResult> SetupNewCloudResourceForUserSocialAccountAsync(string userId, CancellationToken ct = default);
        Task<EcsTaskDefinitionDTO> RegisterTaskDefinitionInAwsAsync(string halId, LeadslyAccountSetupResult result, CancellationToken ct = default);
        Task<CloudMapDiscoveryServiceDTO> CreateCloudMapDiscoveryServiceInAwsAsync(LeadslyAccountSetupResult result, CancellationToken ct = default);
        Task<EcsServiceDTO> CreateEcsServiceInAwsAsync(string taskDefinition, string cloudMapServiceArn, LeadslyAccountSetupResult result, CancellationToken ct = default);
        public Task<bool> AreAwsResourcesHealthyAsync(string cloudMapServiceName, CancellationToken ct = default);
        public Task<bool> EnsureEcsServiceTasksAreRunningAsync(string ecsServiceName, string clusterArn, CancellationToken ct = default);
        public Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, string userId, CancellationToken ct = default);
        Task RollbackEcsServiceAsync(string userId, CancellationToken ct = default);
        Task RollbackCloudMapServiceAsync(string userId, CancellationToken ct = default);
        Task RolbackTaskDefinitionRegistrationAsync(string userId, CancellationToken ct = default);
        public Task<ExistingSocialAccountSetupResultDTO> ConnectToExistingCloudResourceAsync(SocialAccount socialAccount, CancellationToken ct = default);
        public Task RemoveUsersSocialAccountCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default);
    }
}
