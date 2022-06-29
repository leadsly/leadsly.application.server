using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICloudPlatformProvider
    {
        Task<EcsTaskDefinition> AddEcsTaskDefinitionAsync(EcsTaskDefinition newEcsTaskDefinition, CancellationToken ct = default);
        Task<EcsService> AddEcsServiceAsync(EcsService newEcsService, CancellationToken ct = default);
        Task<CloudMapDiscoveryService> AddCloudMapDiscoveryServiceAsync(CloudMapDiscoveryService newService, CancellationToken ct = default);
        public Task<NewSocialAccountSetupResult> SetupNewCloudResourceForUserSocialAccountAsync(string userId, CancellationToken ct = default);
        Task<EcsTaskDefinitionDTO> RegisterTaskDefinitionInAwsAsync(string halId, LeadslyAccountSetupResult result, CancellationToken ct = default);
        Task<CloudMapServiceDiscoveryServiceDTO> CreateCloudMapDiscoveryServiceInAwsAsync(LeadslyAccountSetupResult result, CancellationToken ct = default);
        Task<EcsServiceDTO> CreateEcsServiceInAwsAsync(string taskDefinition, string cloudMapServiceArn, LeadslyAccountSetupResult result, CancellationToken ct = default);
        public Task<bool> AreAwsResourcesHealthyAsync(string cloudMapServiceName, CancellationToken ct = default);
        public Task<bool> EnsureEcsServiceTasksAreRunningAsync(string ecsServiceName, string clusterArn, CancellationToken ct = default);
        public Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, string userId, CancellationToken ct = default);
        Task<string> RollbackEcsServiceAsync(CancellationToken ct = default);
        Task<string> RollbackCloudMapServiceAsync(CancellationToken ct = default);
        Task<string> RolbackTaskDefinitionRegistrationAsync(CancellationToken ct = default);
        public Task<ExistingSocialAccountSetupResultDTO> ConnectToExistingCloudResourceAsync(SocialAccount socialAccount, CancellationToken ct = default);
        public Task RemoveUsersSocialAccountCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default);
    }
}
