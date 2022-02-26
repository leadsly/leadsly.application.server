using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Leadsly.Models.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Leadsly.Domain.Converters;
using Leadsly.Domain.ViewModels.Cloud;
using Microsoft.Extensions.Caching.Memory;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        private readonly int WebDriverId_TimeToEvictionInMin_Cache = 3;
        public async Task<TwoFactorAuthResultViewModel> LeadslyTwoFactorAuthAsync(TwoFactorAuthViewModel twoFactorAuth, CancellationToken ct = default)
        {
            TwoFactorAuthResultViewModel result = new()
            {
                Succeeded = false
            };

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = twoFactorAuth.SocialAccountType,
                Username = twoFactorAuth.Username,
                UserId = twoFactorAuth.UserId
            };

            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            if (socialAccount == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.NOT_FOUND,
                    Detail = "No social account found with the given email and user id",
                    Reason = "No social account found"
                });
                return result;
            }

            string webDriverId = string.Empty;
            if (_memoryCache.TryGetValue(socialAccount.Id, out webDriverId) == false)
            {
                _logger.LogError($"Webdriver id does not exist in cache. This means that either previous request to create web driver failed, or this request came in after expiration time of web driver id value, which is set to {WebDriverId_TimeToEvictionInMin_Cache} mins");
                result.Failures.Add(new()
                {
                    Code = Codes.CACHE_ITEM_NOT_FOUND,
                    Reason = "WebDriverId was not found inside the cache",
                    Detail = "Tried getting WebDriverId from cache using social account id"
                });
                return result;
            }

            return await TwoFactorAuthAsync(socialAccount, twoFactorAuth, webDriverId, ct);
        }
        private async Task<TwoFactorAuthResultViewModel> TwoFactorAuthAsync(SocialAccount socialAccount, TwoFactorAuthViewModel twoFactorAuth, string webDriverId, CancellationToken ct = default)
        {
            TwoFactorAuthResultViewModel result = new()
            {
                Succeeded = false
            };

            EnterTwoFactorAuthResults enterTwoFactorAuthResults = await _leadslyHalProvider.EnterTwoFactorAuthAsync(socialAccount.SocialAccountCloudResource, twoFactorAuth, webDriverId, ct);

            if(enterTwoFactorAuthResults.Succeeded == false)
            {
                result.Failures = FailureConverter.ConvertList(enterTwoFactorAuthResults.Failures);
                return result;
            }

            result.Value = HalAuthenticationConverter.Convert(enterTwoFactorAuthResults.Value);
            result.Succeeded = true;
            return result;
        }
        public async Task<ConnectAccountResultViewModel> LeadslyAuthenticateUserAsync(ConnectAccountViewModel connect, CancellationToken ct = default)
        {
            ConnectAccountResultViewModel result = new()
            {
                Succeeded = false
            };

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = connect.SocialAccountType,
                Username = connect.Username,
                UserId = connect.UserId
            };

            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            if (socialAccount == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.NOT_FOUND,
                    Detail = "No social account found with the given email and user id",
                    Reason = "No social account found"
                });
                return result;
            }

            string webDriverId = string.Empty;
            if (_memoryCache.TryGetValue(socialAccount.Id, out webDriverId) == false)
            {
                _logger.LogError($"Webdriver id does not exist in cache. This means that either previous request to create web driver failed, or this request came in after expiration time of web driver id value, which is set to {WebDriverId_TimeToEvictionInMin_Cache} mins");
                result.Failures.Add(new()
                {
                    Code = Codes.CACHE_ITEM_NOT_FOUND,
                    Reason = "WebDriverId was not found inside the cache",
                    Detail = "Tried getting WebDriverId from cache using social account id"
                });
                return result;
            }

            return await AuthenticateUserAsync(socialAccount, connect, webDriverId, ct);
        }
        private async Task<ConnectAccountResultViewModel> AuthenticateUserAsync(SocialAccount socialAccount, ConnectAccountViewModel connect, string webDriverId, CancellationToken ct = default)
        {
            ConnectAccountResultViewModel result = new()
            {
                Succeeded = false
            };

            ConnectUserAccountResult connectResult = await _leadslyHalProvider.ConnectUserAccountAsync(socialAccount.SocialAccountCloudResource, connect, webDriverId, ct);

            if(connectResult.Succeeded == false)
            {
                result.Failures = FailureConverter.ConvertList(connectResult.Failures);
                return result;
            }

            result.Value = HalAuthenticationConverter.Convert(connectResult.Value);
            result.Succeeded = true;
            return result;
        }
        public async Task<RequestNewWebDriverResultViewModel> LeadslyRequestNewWebDriverAsync(RequestNewWebDriverViewModel request, CancellationToken ct = default)
        {
            RequestNewWebDriverResultViewModel result = new()
            {
                Succeeded = false
            };

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = request.SocialAccountType,
                Username = request.Username,
                UserId = request.UserId
            };

            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            if(socialAccount == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.NOT_FOUND,
                    Detail = "No social account found with the given email and user id",
                    Reason = "No social account found"
                });
                return result;
            }

            return await RequestNewWebDriverAsync(socialAccount, ct);
        }
        private async Task<RequestNewWebDriverResultViewModel> RequestNewWebDriverAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            RequestNewWebDriverResultViewModel result = new()
            {
                Succeeded = false
            };

            if(socialAccount.SocialAccountCloudResource == null)
            {
                _logger.LogError("Social account cloud resource information is missing. Cannot continue without it. It was not retrieved from the database");
                result.Failures.Add(new()
                {
                    Code = Codes.CONFIGURATION_DATA_MISSING,
                    Reason = "Social account cloud resource data missing",
                    Detail = "Cloud resources information missing"
                });

                return result;
            }

            InstantiateNewWebDriverResult newWebDriverResult = await _leadslyHalProvider.RequestNewWebDriverInstanceAsync(socialAccount.SocialAccountCloudResource, ct);

            if(newWebDriverResult.Succeeded == false)
            {
                result.Failures = FailureConverter.ConvertList(newWebDriverResult.Failures);
                return result;
            }

            _memoryCache.Set(socialAccount.Id, newWebDriverResult.Value.WebDriverId, TimeSpan.FromMinutes(WebDriverId_TimeToEvictionInMin_Cache));

            result.Value = HalAuthenticationConverter.Convert(newWebDriverResult.Value);            
            result.Succeeded = true;
            return result;
        }
        /// <summary>
        /// This methods, handles the set up of user's env in aws.
        /// </summary>
        /// <param name="setup"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<SetupAccountResultViewModel> LeadslyAccountSetupAsync(SetupAccountViewModel setup, CancellationToken ct = default)
        {
            SetupAccountResultViewModel result = new()
            {
                Succeeded = false
            };

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = setup.SocialAccountType,
                Username = setup.Username,
                UserId = setup.UserId
            };

            // Check if this social account has been registered for this user before
            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            // if null social account hasn't been registered before for this user
            if(socialAccount == null)
            {
                result = LeadslySetupConverter.Convert(await SetupCloudResourcesForNewSocialAccountAsync(socialAccountDTO, ct));
                result.NewUser = true;
            }
            // social account has been registered before check the ecs service task status and get connection info
            else
            {
                result = LeadslySetupConverter.Convert(await ConnectUserToExistingCloudResourcesAsync(socialAccount, ct));
                result.NewUser = false;
            }

            return result;
        }
        private async Task<LeadslySetupResultDTO> ConnectUserToExistingCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            LeadslySetupResultDTO result = new LeadslySetupResultDTO
            {
                Succeeded = false,
                RequiresNewCloudResource = false
            };

            ExistingSocialAccountSetupResultDTO connectingToExistingCloudResourceResult = await _cloudPlatformProvider.ConnectToExistingCloudResourceAsync(socialAccount, ct);

            if(connectingToExistingCloudResourceResult.Succeeded == false)
            {
                await HandleFailedConnectionAttemptToExistingCloudResourceAsync(connectingToExistingCloudResourceResult, socialAccount, ct);
                result.RequiresNewCloudResource = true;
                return result;
            }

            result.Succeeded = true;
            return result;
        }
        private async Task HandleFailedConnectionAttemptToExistingCloudResourceAsync(ExistingSocialAccountSetupResultDTO existingSocialAccountSetupResult, SocialAccount socialAccount, CancellationToken ct = default)
        {
            if (existingSocialAccountSetupResult.EcsServiceActive == false || existingSocialAccountSetupResult.IsHalHealthy == false)
            {
                _logger.LogInformation("Removing cloud resources associated with this social account");
                // if user ecs service is not active, delete the cloud resource from user's social account including task definition and service discovery name                
                await RemoveUsersSocialAccountCloudResourcesAsync(socialAccount, ct);
                await RemoveUsersSocialAccountAndResourcesAsync(socialAccount, ct);
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
        private async Task RemoveUsersSocialAccountAndResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            bool removeSocialAccountAndResources = await _userProvider.RemoveSocialAccountAndResourcesAsync(socialAccount, ct);
            if(removeSocialAccountAndResources == false)
            {
                _logger.LogError("Failed to remove users social account and the associated cloud resources. Manual intervention may be required to remove the resource.");
            }
        }
        private async Task<LeadslySetupResultDTO> SetupCloudResourcesForNewSocialAccountAsync(SocialAccountDTO socialAccountDTO, CancellationToken ct = default)
        {
            LeadslySetupResultDTO result = new()
            {
                Succeeded = false
            };

            NewSocialAccountSetupResult cloudResourceSetupResult = await _cloudPlatformProvider.SetupNewCloudResourceForUserSocialAccountAsync(socialAccountDTO.UserId, ct);
            bool isHealthy = cloudResourceSetupResult.IsHalHealthy;
            _logger.LogInformation("Is hal healthy? {isHealthy}", isHealthy);

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

            NewSocialAccountSetupResult saveNewSocialAccountResult = await SaveNewSocialAccountAsync(cloudResourceSetupResult, ct);
            if(saveNewSocialAccountResult.Succeeded == false)
            {
                await _cloudPlatformProvider.RollbackCloudResourcesAsync(cloudResourceSetupResult, socialAccountDTO.UserId, ct);
                result.Failures = saveNewSocialAccountResult.Failures;
                return result;
            }

            result.Succeeded = true;
            return result;     
        }
        private async Task<NewSocialAccountSetupResult> SaveNewSocialAccountAsync(NewSocialAccountSetupResult newSocialAccountSetup, CancellationToken ct = default)
        {
            NewSocialAccountSetupResult result = new()
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
            
            NewSocialAccountResult newSocialAndResourcesResult = await _userProvider.AddUsersSocialAccountAsync(newSocialAccountAndCloudResources, ct);            
            if (newSocialAndResourcesResult.Succeeded == false)
            {
                result.Failures = newSocialAndResourcesResult.Failures;
                return result;
            }

            result.Succeeded = true;
            return result;
        }        
    }
}
