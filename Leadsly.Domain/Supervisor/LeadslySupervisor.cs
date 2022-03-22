using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Leadsly.Domain.Converters;
using Leadsly.Application.Model.ViewModels.Cloud;
using Microsoft.Extensions.Caching.Memory;
using Leadsly.Application.Model.Requests;
using NewWebDriverRequest = Leadsly.Application.Model.Requests.NewWebDriverRequest;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using Leadsly.Application.Model.Responses.Hal.Interfaces;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {        
        public async Task<HalOperationResultViewModel<T>> LeadslyTwoFactorAuthAsync<T>(TwoFactorAuthRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = request.SocialAccountType,
                UserId = request.UserId,
                Username = request.Username
            };

            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            if (socialAccount == null)
            {
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.NOT_FOUND,
                    Detail = "No social account found with the given email and user id",
                    Reason = "No social account found"
                });
                return result;
            }

            return await EnterTwofactorAuthCodeAsync<T>(socialAccount.SocialAccountCloudResource, request, ct);        
        }
        private async Task<HalOperationResultViewModel<T>> EnterTwofactorAuthCodeAsync<T>(SocialAccountCloudResource resource, TwoFactorAuthRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            HalOperationResult<IEnterTwoFactorAuthCodeResponse> halResult = await _leadslyHalProvider.EnterTwoFactorAuthAsync<IEnterTwoFactorAuthCodeResponse>(resource, request, ct);
            result = HalOperationConverter.Convert<T>(halResult);

            if (halResult.Succeeded == false)
            {
                return result;
            }

            result.Value = (T)ConnectAccountConverter.Convert(halResult.Value);
            // result.Succeeded = true;
            return result;
        }
        public async Task<HalOperationResultViewModel<T>> LeadslyAuthenticateUserAsync<T>(ConnectAccountRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = request.SocialAccountType,
                UserId = request.UserId,
                Username = request.Username
            };

            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            if (socialAccount == null)
            {
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.NOT_FOUND,
                    Detail = "No social account found with the given email and user id",
                    Reason = "No social account found"
                });
                return result;
            }

            return await AuthenticateUserAsync<T>(socialAccount.SocialAccountCloudResource, request, "", ct);
            
        }
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
            if (socialAccount == null)
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
        private async Task<HalOperationResultViewModel<T>> AuthenticateUserAsync<T>(SocialAccountCloudResource resource, ConnectAccountRequest request, string webDriverId, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            HalOperationResult<IConnectAccountResponse> halResult = await _leadslyHalProvider.ConnectUserAccountAsync<IConnectAccountResponse>(resource, request, ct);
            result = HalOperationConverter.Convert<T>(halResult);

            if (halResult.Succeeded == false)
            {   
                return result;
            }

            result.Value = (T)ConnectAccountConverter.Convert(halResult.Value);
            return result;
        }
        [Obsolete("This method is not longer used. We are not creating new chrome instances per campaign, we're using new tabs instead")]
        public async Task<HalOperationResultViewModel<T>> LeadslyRequestNewWebDriverAsync<T>(NewWebDriverRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = request.SocialAccountType,
                UserId = request.UserId,
                Username = request.Username
            };

            SocialAccount socialAccount = await _userProvider.GetRegisteredSocialAccountAsync(socialAccountDTO, ct);

            if (socialAccount == null)
            {
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.NOT_FOUND,
                    Detail = "No social account found with the given email and user id",
                    Reason = "No social account found"
                });
                return result;
            }

            return await RequestNewWebDriverAsync<T>(socialAccount, ct);
        }
        [Obsolete("This method is not longer used. We are not creating new chrome instances per campaign, we're using new tabs instead")]
        private async Task<HalOperationResultViewModel<T>> RequestNewWebDriverAsync<T>(SocialAccount socialAccount, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            if (socialAccount.SocialAccountCloudResource == null)
            {
                _logger.LogError("Social account cloud resource information is missing. Cannot continue without it. It was not retrieved from the database");
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.CONFIGURATION_DATA_MISSING,
                    Reason = "Social account cloud resource data missing",
                    Detail = "Cloud resources information missing"
                });

                return result;
            }

            HalOperationResult<INewWebDriverResponse> halResult = await _leadslyHalProvider.RequestNewWebDriverInstanceAsync<INewWebDriverResponse>(socialAccount.SocialAccountCloudResource, ct);
            result = HalOperationConverter.Convert<T>(halResult);

            if (halResult.Succeeded == false)
            {
                return result;
            }

            result.Value = (T)WebDriverConverter.Convert(halResult.Value);
            //result.Succeeded = true;
            return result;
        }           
        /// <summary>
        /// This methods, handles the set up of user's env in aws.
        /// </summary>
        /// <param name="setup"></param>
        /// <param name="ct"></param>
        /// <returns></returns>        
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
