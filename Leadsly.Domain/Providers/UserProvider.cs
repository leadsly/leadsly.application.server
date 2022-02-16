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
        public UserProvider(ILogger<UserProvider> logger, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        private readonly ILogger<UserProvider> _logger;
        private readonly IUserRepository _userRepository;

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

        public async Task<SocialAccount> AddUsersSocialAccountAsync(SocialAccountAndResourcesDTO newSocialAccountSetup, CancellationToken ct = default)
        {
            SocialAccount newSocialAccount = default;
            try
            {
                newSocialAccount = new()
                {
                    AccountType = newSocialAccountSetup.AccountType,
                    Username = newSocialAccountSetup.Username,
                    UserId = newSocialAccountSetup.UserId,
                    SocialAccountCloudResource = new()
                    {
                        CloudMapServiceDiscoveryService = new()
                        {
                            Arn = newSocialAccountSetup.Value.CloudMapServiceDiscovery.Arn,
                            Name = newSocialAccountSetup.Value.CloudMapServiceDiscovery.Name
                        },
                        EcsService = new()
                        {
                            AssignPublicIp = newSocialAccountSetup.Value.EcsService.AssignPublicIp,
                            ClusterArn = newSocialAccountSetup.Value.EcsService.ClusterArn,
                            CreatedAt = ((DateTimeOffset)newSocialAccountSetup.Value.EcsService.CreatedAt).ToUnixTimeSeconds(),
                            CreatedBy = newSocialAccountSetup.Value.EcsService.CreatedBy,
                            DesiredCount = newSocialAccountSetup.Value.EcsService.DesiredCount,
                            EcsServiceRegistries = newSocialAccountSetup.Value.EcsService.Registries.Select(r => new EcsServiceRegistry
                            {
                                RegistryArn = r.RegistryArn
                            }).ToList(),
                            SchedulingStrategy = newSocialAccountSetup.Value.EcsService.SchedulingStrategy,
                            ServiceArn = newSocialAccountSetup.Value.EcsService.ServiceArn,
                            ServiceName = newSocialAccountSetup.Value.EcsService.ServiceName,
                            TaskDefinition = newSocialAccountSetup.Value.EcsService.TaskDefinition,
                            UserId = newSocialAccountSetup.Value.EcsService.UserId
                        },
                        EcsTaskDefinition = new()
                        {
                            Family = newSocialAccountSetup.Value.EcsTaskDefinition.Family,
                            ContainerName = newSocialAccountSetup.Value.EcsTaskDefinition.ContainerName
                        }
                    }

                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "An error occured while creating new social account object.");
                //result.Succeeded = false;
                //result.Failures.Add(new()
                //{
                //    Reason = "Failed to perform object mapping.",
                //    Detail = "Something went during object mapping. Check server Logs."
                //});
                //return result;
            }

            // first add Ecs Task Definition

            // then

            return null;
        }
    }
}
