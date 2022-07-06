using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
using Microsoft.Extensions.Logging;
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

        public async Task<ConnectLinkedInAccountResultViewModel> LinkLinkedInAccount(ConnectLinkedInAccountRequest request, string userId, CancellationToken ct = default)
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

            if (string.IsNullOrEmpty(virtualAssistant.CloudMapDiscoveryService?.Name))
            {
                _logger.LogError("Couldn't process request to link account because no cloud map discovery service has been found. This name is required to make a request to hal");
                return null;
            }

            ConnectLinkedInAccountResponse response = await _leadslyHalProvider.ConnectAccountAsync(request.Username, request.Password, virtualAssistant.CloudMapDiscoveryService.Name, ct);
            if (response == null)
            {
                return null;
            }

            if (response.TwoFactorAuthRequired == false && response.UnexpectedErrorOccured == false && response.InvalidCredentials == false)
            {
                // assume we've successfully linked the account so now we can create new social account for the user

                SocialAccount socialAccount = await _userProvider.CreateSocialAccountAsync(virtualAssistant, userId, request.Username, ct);
                if (socialAccount == null)
                {
                    return null;
                }
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

            if (string.IsNullOrEmpty(virtualAssistant.CloudMapDiscoveryService?.Name))
            {
                _logger.LogError("Couldn't process request to link account because no cloud map discovery service has been found. This name is required to make a request to hal");
                return null;
            }

            EnterTwoFactorAuthResponse response = await _leadslyHalProvider.EnterTwoFactorAuthAsync(request.Code, virtualAssistant.CloudMapDiscoveryService.Name, ct);
            if (response == null)
            {
                return null;
            }

            if (response.InvalidOrExpiredCode == false && response.UnexpectedErrorOccured == false && response.FailedToEnterCode == false)
            {
                // assume we've successfully linked the account so now we can create new social account for the user
                SocialAccount socialAccount = await _userProvider.CreateSocialAccountAsync(virtualAssistant, userId, request.Code, ct);
                if (socialAccount == null)
                {
                    return null;
                }
            }

            TwoFactorAuthResultViewModel viewModel = LinkedInSetupConverter.Convert(response);
            return viewModel;
        }
    }
}
