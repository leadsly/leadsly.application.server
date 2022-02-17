using Leadsly.Domain.Converters;
using Leadsly.Domain.Repositories;
using Leadsly.Models;
using Leadsly.Models.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class UserProvider : IUserProvider
    {
        public UserProvider(ILogger<UserProvider> logger, ICloudPlatformRepository cloudPlatformRepository, IUserRepository userRepository, ISocialAccountRepository socialAccountRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
            _cloudPlatformRepository = cloudPlatformRepository;
            _socialAccountRepository = socialAccountRepository;

        }

        private readonly ILogger<UserProvider> _logger;
        private readonly IUserRepository _userRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ISocialAccountRepository _socialAccountRepository;

        public async Task<SocialAccount> GetRegisteredSocialAccountAsync(SocialAccountDTO socialAccountDTO, CancellationToken ct = default)
        {
            IEnumerable<SocialAccount> socialAccounts = await _userRepository.GetSocialAccountsByUserIdAsync(socialAccountDTO.UserId, ct);
            if(socialAccounts == null)
            {
                return null;
            }

            SocialAccount socialAccount = socialAccounts.FirstOrDefault(s => s.Username == socialAccountDTO.Username);
            return socialAccount;
        }

        public async Task<bool> RemoveSocialAccountAndResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {            
            bool removeSocialAccountResourcesDelete = await RemoveSocialAccountResourcesAsync(socialAccount.SocialAccountCloudResource, ct);

            bool removeSocialAccountDelete = await RemoveSocialAccountAsync(socialAccount.Id, ct);

            return removeSocialAccountResourcesDelete && removeSocialAccountDelete;
        }

        private async Task<bool> RemoveSocialAccountAsync(string socialAccountId, CancellationToken ct = default)
        {
            bool socialAccountDelete = await _socialAccountRepository.RemoveSocialAccountAsync(socialAccountId, ct);

            return socialAccountDelete;
        }

        private async Task<bool> RemoveSocialAccountResourcesAsync(SocialAccountCloudResource resources, CancellationToken ct = default)
        {            
            bool ecsTaskDefinitionDelete = await _cloudPlatformRepository.RemoveEcsTaskDefinitionAsync(resources.EcsTaskDefinition.Id, ct);

            bool serviceDiscoveryDelete = await _cloudPlatformRepository.RemoveCloudMapServiceDiscoveryServiceAsync(resources.CloudMapServiceDiscoveryService.Id, ct);

            bool ecsServiceDelete = await _cloudPlatformRepository.RemoveEcsServiceAsync(resources.EcsService.Id, ct);

            return ecsServiceDelete && ecsTaskDefinitionDelete && serviceDiscoveryDelete;
        }

        public async Task<NewSocialAccountResult> AddUsersSocialAccountAsync(SocialAccountAndResourcesDTO newSocialAccountSetup, CancellationToken ct = default)
        {
            NewSocialAccountResult result = new()
            {
                Succeeded = false
            };

            EcsTaskDefinition newEcsTaskDefinition = await AddEcsTaskDefinitionAsync(EcsTaskDefinitionConverter.Convert(newSocialAccountSetup.Value.EcsTaskDefinition), ct);

            if(newEcsTaskDefinition == null)
            {
                result.Failures.Add(new()
                {
                    Arn = newSocialAccountSetup.Value.EcsTaskDefinition.TaskDefinitionArn,
                    Detail = "Something went wrong adding ecs task definition to the database",
                    Reason = "Failed to add ecs task definition to the database",                    
                });
                return result;
            }

            EcsService ecsService = await AddEcsServiceAsync(EcsServiceConverter.Convert(newSocialAccountSetup.Value.EcsService), ct);

            if(ecsService == null)
            {
                result.Failures.Add(new()
                {
                    Arn = newSocialAccountSetup.Value.EcsService.ServiceArn,
                    Detail = "Something went wrong adding ecs service to the database",
                    Reason = "Failed to add ecs service to the database",
                });
                return result;
            }

            CloudMapServiceDiscoveryService newServiceDiscovery = CloudMapServiceDiscoveryServiceConverter.Convert(newSocialAccountSetup.Value.CloudMapServiceDiscovery);
            newServiceDiscovery.EcsService = ecsService;
            newServiceDiscovery = await AddServiceDiscoveryServiceAsync(newServiceDiscovery, ct);

            if(newServiceDiscovery == null)
            {
                result.Failures.Add(new()
                {
                    Arn = newSocialAccountSetup.Value.CloudMapServiceDiscovery.Arn,
                    Detail = "Something went wrong adding service discovery to the database",
                    Reason = "Failed to add discovery service the database",
                });
                return result;
            }

            SocialAccountCloudResource resource = new()
            {
                EcsService = ecsService,
                CloudMapServiceDiscoveryService = newServiceDiscovery,
                EcsTaskDefinition = newEcsTaskDefinition,
                HalsUniqueName = newSocialAccountSetup.Value.HalsUniqueName
            };

            SocialAccount newSocialAccount = new()
            {
                AccountType = newSocialAccountSetup.AccountType,
                Username = newSocialAccountSetup.Username,
                UserId = newSocialAccountSetup.UserId,
                SocialAccountCloudResource = resource
            };
            newSocialAccount = await AddSocialAccountAsync(newSocialAccount, ct);

            if(newSocialAccount == null)
            {
                result.Failures.Add(new()
                {
                    Detail = "Something went wrong adding user's social account to the database",
                    Reason = "Failed to add user's social account to the database",
                });
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
        private async Task<CloudMapServiceDiscoveryService> AddServiceDiscoveryServiceAsync(CloudMapServiceDiscoveryService newCloudMapServiceDiscovery, CancellationToken ct = default)
        {
            newCloudMapServiceDiscovery = await _cloudPlatformRepository.AddServiceDiscoveryAsync(newCloudMapServiceDiscovery, ct);

            return newCloudMapServiceDiscovery;
        }
    }
}
