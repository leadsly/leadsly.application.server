using Leadsly.Domain.Converters;
using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<ConnectedViewModel> GetConnectedAccountAsync(string userId, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.GetVirtualAssistantAsync(userId, ct);
            if (virtualAssistant == null)
            {
                return new ConnectedViewModel
                {
                    IsConnected = false,
                    ConnectedAccount = null
                };
            }
            else
            {
                return new ConnectedViewModel
                {
                    IsConnected = virtualAssistant.SocialAccount == null ? false : virtualAssistant.SocialAccount.Linked,
                    ConnectedAccount = virtualAssistant.SocialAccount == null ? null : virtualAssistant.ToConnectedAccountViewModel()
                };
            }
        }

        public async Task<ConnectLinkedInAccountResultViewModel> LinkLinkedInAccount(ConnectLinkedInAccountRequest request, string userId, IHeaderDictionary responseHeaders, IHeaderDictionary requestHeaders, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.GetVirtualAssistantAsync(userId, ct);
            if (virtualAssistant == null)
            {
                _logger.LogError("Couldn't process request to link account because no virtual assistant has been created");
                return null;
            }

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogError("Couldn't process request to link account because username or password is empty");
                return null;
            }

            if (virtualAssistant.CloudMapDiscoveryServices == null || virtualAssistant.CloudMapDiscoveryServices.Count == 0)
            {
                _logger.LogError("Couldn't process request to link account because no cloud map discovery services has been found. This name is required to make a request to hal");
                return null;
            }

            EcsService halEcsService = virtualAssistant.EcsServices.Where(x => x.Purpose == EcsResourcePurpose.Hal).FirstOrDefault();
            if (halEcsService == null)
            {
                _logger.LogError("Couldn't process request to link account because no hal ecs service has been found. This service is required to make a request to hal");
                return null;
            }

            EcsService gridEcsService = virtualAssistant.EcsServices.Where(x => x.Purpose == EcsResourcePurpose.Grid).FirstOrDefault();
            if (halEcsService == null)
            {
                _logger.LogError("Couldn't process request to link account because no grid ecs service has been found. This service is required to make a request to hal");
                return null;
            }

            EcsService proxyEcsService = virtualAssistant.EcsServices.Where(x => x.Purpose == EcsResourcePurpose.Proxy).FirstOrDefault();
            if (halEcsService == null)
            {
                _logger.LogError("Couldn't process request to link account because no proxy ecs service has been found. This service is required to make a request to linkedin");
                return null;
            }

            ConnectLinkedInAccountResponse response = await _leadslyHalProvider.ConnectAccountAsync(request.Username, request.Password, halEcsService.CloudMapDiscoveryService.Name, gridEcsService.CloudMapDiscoveryService.Name, proxyEcsService.CloudMapDiscoveryService.Name, responseHeaders, requestHeaders, ct);
            if (response == null)
            {
                return null;
            }

            if (response.TwoFactorAuthRequired == false && response.UnexpectedErrorOccured == false && response.InvalidEmail == false && response.InvalidPassword == false && response.EmailPinChallenge == false)
            {
                // assume we've successfully linked the account so now we can create new social account for the user
                SocialAccount socialAccount = await CreateSocialAccountAndSaveBrowserProfileAsync(virtualAssistant, userId, request.Username, ct);
                if (socialAccount == null)
                {
                    return null;
                }

                // deprovision resources
                await _deprovisionResourcesProvider.DeprovisionResourcesAsync(virtualAssistant.HalId, ct);
            }

            ConnectLinkedInAccountResultViewModel viewModel = LinkedInSetupConverter.Convert(response);
            return viewModel;

        }

        public async Task<TwoFactorAuthResultViewModel> EnterTwoFactorAuthAsync(string userId, TwoFactorAuthRequest request, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.GetVirtualAssistantAsync(userId, ct);
            if (virtualAssistant == null)
            {
                _logger.LogError("Couldn't process request to enter two factor auth because no virtual assistant has been created");
                return null;
            }

            if (string.IsNullOrEmpty(request.Code))
            {
                _logger.LogError("Couldn't process request to enter two factor auth because two factor auth code is empty");
                return null;
            }

            if (virtualAssistant.CloudMapDiscoveryServices == null || virtualAssistant.CloudMapDiscoveryServices.Count == 0)
            {
                _logger.LogError("Couldn't process request to link account because no cloud map discovery service has been found. This name is required to make a request to hal");
                return null;
            }

            EcsService halEcsService = virtualAssistant.EcsServices.Where(x => x.Purpose == EcsResourcePurpose.Hal).FirstOrDefault();
            if (halEcsService == null)
            {
                _logger.LogError("Couldn't process request to link account because no hal ecs service has been found. This service is required to make a request to hal");
                return null;
            }

            EcsService gridEcsService = virtualAssistant.EcsServices.Where(x => x.Purpose == EcsResourcePurpose.Grid).FirstOrDefault();
            if (halEcsService == null)
            {
                _logger.LogError("Couldn't process request to link account because no grid ecs service has been found. This service is required to make a request to hal");
                return null;
            }

            EnterTwoFactorAuthResponse response = await _leadslyHalProvider.EnterTwoFactorAuthAsync(request.Code, halEcsService.CloudMapDiscoveryService.Name, gridEcsService.CloudMapDiscoveryService.Name, ct);
            if (response == null)
            {
                return null;
            }

            if (response.InvalidOrExpiredCode == false && response.UnexpectedErrorOccured == false && response.FailedToEnterCode == false)
            {
                // assume we've successfully linked the account so now we can create new social account for the user
                SocialAccount socialAccount = await CreateSocialAccountAndSaveBrowserProfileAsync(virtualAssistant, userId, request.Username, ct);
                if (socialAccount == null)
                {
                    return null;
                }

                // deprovision resources
                await _deprovisionResourcesProvider.DeprovisionResourcesAsync(virtualAssistant.HalId, ct);
            }

            TwoFactorAuthResultViewModel viewModel = LinkedInSetupConverter.Convert(response);
            return viewModel;
        }

        public async Task<EmailChallengePinResultViewModel> EnterEmailChallengePinAsync(string userId, EmailChallengePinRequest request, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.GetVirtualAssistantAsync(userId, ct);
            if (virtualAssistant == null)
            {
                _logger.LogError("Couldn't process request to enter email challenge pin because no virtual assistant has been created");
                return null;
            }

            if (string.IsNullOrEmpty(request.Pin))
            {
                _logger.LogError("Couldn't process request to enter enter email challenge pin because email challenge pin is empty");
                return null;
            }

            if (virtualAssistant.CloudMapDiscoveryServices == null || virtualAssistant.CloudMapDiscoveryServices.Count == 0)
            {
                _logger.LogError("Couldn't process request to link account because no cloud map discovery service has been found. This name is required to make a request to hal");
                return null;
            }

            EcsService halEcsService = virtualAssistant.EcsServices.Where(x => x.Purpose == EcsResourcePurpose.Hal).FirstOrDefault();
            if (halEcsService == null)
            {
                _logger.LogError("Couldn't process request to link account because no hal ecs service has been found. This service is required to make a request to hal");
                return null;
            }

            EcsService gridEcsService = virtualAssistant.EcsServices.Where(x => x.Purpose == EcsResourcePurpose.Grid).FirstOrDefault();
            if (halEcsService == null)
            {
                _logger.LogError("Couldn't process request to link account because no grid ecs service has been found. This service is required to make a request to hal");
                return null;
            }

            EnterEmailChallengePinResponse response = await _leadslyHalProvider.EnterEmailChallengePinAsync(request.Pin, halEcsService.CloudMapDiscoveryService.Name, gridEcsService.CloudMapDiscoveryService.Name, ct);
            if (response == null)
            {
                _logger.LogError("Response from hal was null");
                return null;
            }

            if (response.InvalidOrExpiredPin == false && response.UnexpectedErrorOccured == false && response.FailedToEnterPin == false && response.TwoFactorAuthRequired == false)
            {
                string email = request.Username;
                _logger.LogInformation("Successfully entered email challenge pin and two factor auth is not required. Creating social account for UserId {userId} with email {email}", userId, email);
                // assume we've successfully linked the account so now we can create new social account for the user
                SocialAccount socialAccount = await CreateSocialAccountAndSaveBrowserProfileAsync(virtualAssistant, userId, request.Username, ct);
                if (socialAccount == null)
                {
                    _logger.LogError("Failed to create social account for UserId {userId} with email {email}", userId, email);
                    return null;
                }

                // deprovision resources
                await _deprovisionResourcesProvider.DeprovisionResourcesAsync(virtualAssistant.HalId, ct);
            }

            EmailChallengePinResultViewModel viewModel = LinkedInSetupConverter.Convert(response);
            return viewModel;
        }

        private async Task<SocialAccount> CreateSocialAccountAndSaveBrowserProfileAsync(VirtualAssistant virtualAssistant, string userId, string email, CancellationToken ct = default)
        {
            return await _saveBrowserProfileUserProvider.CreateSocialAccountAsync(virtualAssistant, userId, email, ct);
        }
    }
}
