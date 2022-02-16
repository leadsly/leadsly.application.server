using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Leadsly.Models.Aws;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<LeadslyConnectionResult> ConnectAccountToLeadslyAsync(ConnectLeadslyViewModel connectAccount, CancellationToken ct = default)
        {
            await _leadslyBotApiService.ConnectToLeadslyAsync(connectAccount, ct);

            LeadslyConnectionResult result = new LeadslyConnectionResult();

            return result;
        }

        /// <summary>
        /// This methods, handles the set up of user's env in aws.
        /// </summary>
        /// <param name="setup"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<LeadslyConnectResultDTO> SetupLeadslyForUserAsync(ConnectUserDTO setup, CancellationToken ct = default)
        {
            LeadslyConnectResultDTO result = new()
            {
                Succeeded = false
            };

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = setup.SocialAccountType,
                Username = setup.Username,
                Password = setup.Password,
                UserId = setup.UserId
            };

            // Check if this social account has been registered for this user before
            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            // if null social account hasn't been registered before for this user
            if(socialAccount == null)
            {
                result = await SetupCloudResourcesForNewSocialAccountAsync(socialAccountDTO, ct);
            }
            // social account has been registered before check the ecs service task status and get connection info
            else
            {
                result = await ConnectUserToExistingCloudResourcesAsync(socialAccount, ct);
            }

            return result;
        }

        private async Task<LeadslyConnectResultDTO> ConnectUserToExistingCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            LeadslyConnectResultDTO result = new LeadslyConnectResultDTO
            {
                Succeeded = false,
                RequiresNewCloudResource = false
            };

            ExistingSocialAccountSetupResult connectingToExistingCloudResourceResult = await _cloudPlatformProvider.ConnectToExistingCloudResourceAsync(socialAccount, ct);

            if(connectingToExistingCloudResourceResult.Succeeded == false)
            {
                await HandleFailedConnectionAttemptToExistingCloudResourceAsync(connectingToExistingCloudResourceResult, socialAccount, ct);
                result.RequiresNewCloudResource = true;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private async Task HandleFailedConnectionAttemptToExistingCloudResourceAsync(ExistingSocialAccountSetupResult existingSocialAccountSetupResult, SocialAccount socialAccount, CancellationToken ct = default)
        {
            if (existingSocialAccountSetupResult.EcsServiceActive == false || existingSocialAccountSetupResult.IsHalHealthy == false)
            {
                _logger.LogInformation("Removing cloud resources associated with this social account");
                // if user ecs service is not active, delete the cloud resource from user's social account including task definition and service discovery name
                await RemoveUsersSocialAccountAsync(socialAccount.Id, ct);
                await RemoveUsersSocialAccountCloudResourcesAsync(socialAccount, ct);
            }

            if (existingSocialAccountSetupResult.EcsTaskRunning == false && existingSocialAccountSetupResult.EcsServiceHasPendingTasks == true)
            {
                // this should happen very rarely
                _logger.LogWarning("[EDGE CASE]: Ecs service has no running tasks but has a pending task. There is no logic created for this scenario yet.");
               // its possible that the ecs service has no tasks running but it has a task in pending state even after the default timeout time. Just log as warning for now
            }
        }


        private async Task RemoveUsersSocialAccountCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            await _cloudPlatformProvider.RemoveUsersSocialAccountCloudResourcesAsync(socialAccount, ct);
        }
        private async Task RemoveUsersSocialAccountAsync(string userSocialAccountId, CancellationToken ct = default)
        {
            bool removeSocialAccount = await _socialAccountRepository.RemoveSocialAccountAsync(userSocialAccountId, ct);
            if(removeSocialAccount == false)
            {
                _logger.LogError("Failed to remove users social account and the associated cloud resources. Manual intervention may be required to remove the resource.");
            }
        }

        private async Task<LeadslyConnectResultDTO> SetupCloudResourcesForNewSocialAccountAsync(SocialAccountDTO socialAccountDTO, CancellationToken ct = default)
        {
            LeadslyConnectResultDTO result = new()
            {
                Succeeded = false
            };

            NewSocialAccountSetupResult cloudResourceSetupResult = await _cloudPlatformProvider.SetupNewCloudResourceForUserSocialAccountAsync(socialAccountDTO.UserId, ct);
            if (cloudResourceSetupResult.Succeeded == false)
            {
                // clean up aws resources
                await _cloudPlatformProvider.RollbackCloudResourcesAsync(cloudResourceSetupResult, socialAccountDTO.UserId, ct);
                result.Failures = cloudResourceSetupResult.Failures;
                return result;
            }

            // persist to the database the container name, discovery service name, user social account
            cloudResourceSetupResult.Username = socialAccountDTO.Username;
            cloudResourceSetupResult.Password = socialAccountDTO.Password;
            cloudResourceSetupResult.UserId = socialAccountDTO.UserId;
            cloudResourceSetupResult.AccountType = socialAccountDTO.AccountType;

            return await SaveNewSocialAccountAsync(cloudResourceSetupResult, ct);          
        }

        private async Task<LeadslyConnectResultDTO> SaveNewSocialAccountAsync(NewSocialAccountSetupResult newSocialAccountSetup, CancellationToken ct = default)
        {
            LeadslyConnectResultDTO result = new()
            {
                Succeeded = false
            };

            SocialAccountAndResourcesDTO newSocialAccountAndCloudResources = new()
            {
                AccountType = newSocialAccountSetup.AccountType,
                Password = newSocialAccountSetup.Password,
                UserId = newSocialAccountSetup.UserId,
                Username = newSocialAccountSetup.Username,
                Value = newSocialAccountSetup.Value
            };

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
                _logger.LogError(ex, "An error occured while creating new social account object.");
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Reason = "Failed to perform object mapping.",
                    Detail = "Something went during object mapping. Check server Logs."
                });
                return result;
            }

            // userAccount provider
            //var newSocialAndResourcesResult = await _userProvider.AddUsersSocialAccountAsync(newSocialAccountAndCloudResources, ct);
            newSocialAccount = await _socialAccountRepository.AddSocialAccountAsync(newSocialAccount, ct);
            if (newSocialAccount == null)
            {
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Detail = "Something went wrong when adding social account to the database. Check server logs",
                    Reason = "Failed to add new social account to the database"
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }        
    }
}
