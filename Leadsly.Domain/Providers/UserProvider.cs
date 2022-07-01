using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Converters;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class UserProvider : IUserProvider
    {
        public UserProvider(
            ILogger<UserProvider> logger,
            ICloudPlatformRepository cloudPlatformRepository,
            IUserRepository userRepository,
            IScanProspectsForRepliesPhaseRepository scanProspectsForRepliesPhaseRepository,
            ISocialAccountRepository socialAccountRepository,
            IConnectionWithdrawPhaseRepository connectionWithdrawPhaseRepository,
            IMonitorForNewConnectionsPhaseRepository monitorForNewConnectionsPhaseRepository,
            ICampaignRepository campaignRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
            _scanProspectsForRepliesPhaseRepository = scanProspectsForRepliesPhaseRepository;
            _connectionWithdrawPhaseRepository = connectionWithdrawPhaseRepository;
            _cloudPlatformRepository = cloudPlatformRepository;
            _monitorForNewConnectionsPhaseRepository = monitorForNewConnectionsPhaseRepository;
            _socialAccountRepository = socialAccountRepository;
            _campaignRepository = campaignRepository;
        }

        private readonly ILogger<UserProvider> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IScanProspectsForRepliesPhaseRepository _scanProspectsForRepliesPhaseRepository;
        private readonly IConnectionWithdrawPhaseRepository _connectionWithdrawPhaseRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMonitorForNewConnectionsPhaseRepository _monitorForNewConnectionsPhaseRepository;

        public async Task<SocialAccount> GetRegisteredSocialAccountAsync(SocialAccountDTO socialAccountDTO, CancellationToken ct = default)
        {
            IEnumerable<SocialAccount> socialAccounts = await _userRepository.GetSocialAccountsByUserIdAsync(socialAccountDTO.UserId, ct);
            if (socialAccounts == null)
            {
                return null;
            }

            SocialAccount socialAccount = socialAccounts.FirstOrDefault(s => s.Username == socialAccountDTO.Username);
            return socialAccount;
        }

        public async Task<bool> RemoveSocialAccountAndResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            bool removeSocialAccountResourcesDelete = await RemoveSocialAccountResourcesAsync(socialAccount.SocialAccountCloudResource, ct);

            bool removeSocialAccountDelete = await RemoveSocialAccountAsync(socialAccount.SocialAccountId, ct);

            return removeSocialAccountResourcesDelete && removeSocialAccountDelete;
        }

        private async Task<bool> RemoveSocialAccountAsync(string socialAccountId, CancellationToken ct = default)
        {
            bool socialAccountDelete = await _socialAccountRepository.RemoveSocialAccountAsync(socialAccountId, ct);

            return socialAccountDelete;
        }

        private async Task<bool> RemoveSocialAccountResourcesAsync(SocialAccountCloudResource resources, CancellationToken ct = default)
        {
            bool ecsTaskDefinitionDelete = await _cloudPlatformRepository.RemoveEcsTaskDefinitionAsync(resources.EcsTaskDefinition.EcsTaskDefinitionId, ct);

            //bool serviceDiscoveryDelete = await _cloudPlatformRepository.RemoveCloudMapServiceDiscoveryServiceAsync(resources.CloudMapServiceDiscoveryService.CloudMapServiceDiscoveryServiceId, ct);

            bool ecsServiceDelete = await _cloudPlatformRepository.RemoveEcsServiceAsync(resources.EcsService.EcsServiceId, ct);

            return ecsServiceDelete && ecsTaskDefinitionDelete; // && serviceDiscoveryDelete;
        }

        public async Task<NewSocialAccountResult> AddUsersSocialAccountAsync(SocialAccountAndResourcesDTO newSocialAccountSetup, CancellationToken ct = default)
        {
            NewSocialAccountResult result = new()
            {
                Succeeded = false
            };

            EcsTaskDefinition newEcsTaskDefinition = await AddEcsTaskDefinitionAsync(EcsTaskDefinitionConverter.Convert(newSocialAccountSetup.Value.EcsTaskDefinition), ct);

            if (newEcsTaskDefinition == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.DATABASE_OPERATION_ERROR,
                    Arn = newSocialAccountSetup.Value.EcsTaskDefinition.TaskDefinitionArn,
                    Detail = "Something went wrong adding ecs task definition to the database",
                    Reason = "Failed to add ecs task definition to the database",
                });
                return result;
            }

            EcsService ecsService = await AddEcsServiceAsync(EcsServiceConverter.Convert(newSocialAccountSetup.Value.EcsService), ct);

            if (ecsService == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.DATABASE_OPERATION_ERROR,
                    Arn = newSocialAccountSetup.Value.EcsService.ServiceArn,
                    Detail = "Something went wrong adding ecs service to the database",
                    Reason = "Failed to add ecs service to the database",
                });
                return result;
            }

            CloudMapDiscoveryService newServiceDiscovery = CloudMapDiscoveryServiceConverter.Convert(newSocialAccountSetup.Value.CloudMapServiceDiscovery);
            newServiceDiscovery.EcsService = ecsService;
            newServiceDiscovery = await AddServiceDiscoveryServiceAsync(newServiceDiscovery, ct);

            if (newServiceDiscovery == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.DATABASE_OPERATION_ERROR,
                    Arn = newSocialAccountSetup.Value.CloudMapServiceDiscovery.Arn,
                    Detail = "Something went wrong adding service discovery to the database",
                    Reason = "Failed to add discovery service the database",
                });
                return result;
            }

            SocialAccountCloudResource resource = new()
            {
                EcsService = ecsService,
                //CloudMapServiceDiscoveryService = newServiceDiscovery,
                EcsTaskDefinition = newEcsTaskDefinition,
                HalId = newSocialAccountSetup.Value.HalId
            };

            SocialAccount newSocialAccount = new()
            {
                AccountType = newSocialAccountSetup.AccountType,
                Username = newSocialAccountSetup.Username,
                UserId = newSocialAccountSetup.UserId,
                SocialAccountCloudResource = resource
            };
            newSocialAccount = await AddSocialAccountAsync(newSocialAccount, ct);

            if (newSocialAccount == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.DATABASE_OPERATION_ERROR,
                    Detail = "Something went wrong adding user's social account to the database",
                    Reason = "Failed to add user's social account to the database",
                });
                return result;
            }

            ScanProspectsForRepliesPhase scanForProspectRepliesPhase = new()
            {
                SocialAccount = newSocialAccount,
                PhaseType = PhaseType.ScanForReplies
            };

            scanForProspectRepliesPhase = await _scanProspectsForRepliesPhaseRepository.CreateAsync(scanForProspectRepliesPhase, ct);
            if (scanForProspectRepliesPhase == null)
            {
                return result;
            }

            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = new()
            {
                SocialAccount = newSocialAccount,
                PhaseType = PhaseType.MonitorNewConnections
            };

            monitorForNewConnectionsPhase = await _monitorForNewConnectionsPhaseRepository.CreateAsync(monitorForNewConnectionsPhase, ct);
            if (monitorForNewConnectionsPhase == null)
            {
                return result;
            }

            ConnectionWithdrawPhase connectionWithdrawPhase = new()
            {
                PhaseType = PhaseType.ConnectionWithdraw,
                SocialAccount = newSocialAccount
            };

            connectionWithdrawPhase = await _connectionWithdrawPhaseRepository.CreateAsync(connectionWithdrawPhase, ct);
            if (connectionWithdrawPhase == null)
            {
                return result;
            }

            result.Succeeded = true;
            result.Value = newSocialAccount;
            return result;
        }
        private async Task<SocialAccount> AddSocialAccountAsync(SocialAccount newSocialAccount, CancellationToken ct = default)
        {
            newSocialAccount = await _socialAccountRepository.AddSocialAccountAsync(newSocialAccount, ct);

            return newSocialAccount;
        }
        private async Task<EcsTaskDefinition> AddEcsTaskDefinitionAsync(EcsTaskDefinition newEcsTaskDefinition, CancellationToken ct = default)
        {
            newEcsTaskDefinition = await _cloudPlatformRepository.AddEcsTaskDefinitionAsync(newEcsTaskDefinition, ct);

            return newEcsTaskDefinition;
        }
        private async Task<EcsService> AddEcsServiceAsync(EcsService newEcsService, CancellationToken ct = default)
        {
            newEcsService = await _cloudPlatformRepository.AddEcsServiceAsync(newEcsService, ct);

            return newEcsService;
        }
        private async Task<CloudMapDiscoveryService> AddServiceDiscoveryServiceAsync(CloudMapDiscoveryService newCloudMapServiceDiscovery, CancellationToken ct = default)
        {
            newCloudMapServiceDiscovery = await _cloudPlatformRepository.AddServiceDiscoveryAsync(newCloudMapServiceDiscovery, ct);

            return newCloudMapServiceDiscovery;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId, CancellationToken ct = default)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<IList<SocialAccount>> GetAllSocialAccounts(CancellationToken ct = default)
        {
            return await _userRepository.GetAllSocialAccountsAsync(ct);
        }

        public async Task<SocialAccount> GetSocialAccountByHalIdAsync(string halId, CancellationToken ct = default)
        {
            return await _userRepository.GetSocialAccountByHalIdAsync(halId, ct);
        }
    }
}
