using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Models.ViewModels.Hal;
using Leadsly.Models;
using Leadsly.Models.Entities;
using Leadsly.Models.Requests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Models.ViewModels.Interfaces;
using Leadsly.Models.Requests.Hal;
using Leadsly.Models.Respones.Hal;

namespace Leadsly.Domain.Providers
{
    public class LeadslyHalProvider : ILeadslyHalProvider
    {
        public LeadslyHalProvider(ILeadslyHalApiService leadslyHalApiService, ICloudPlatformRepository cloudPlatformRepository, ILogger<LeadslyHalProvider> logger)
        {
            _leadslyHalApiService = leadslyHalApiService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _logger = logger;
        }

        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILeadslyHalApiService _leadslyHalApiService;
        private readonly ILogger<LeadslyHalProvider> _logger;
        private readonly string RequestNewWebDriverUrl = "api/webdriver";
        private readonly string AuthenticateUserSocialAccount = "api/authentication";
        private readonly string AuthenticateUserSocialAccount2Fa = "api/authentication/2fa";
        private readonly int DefaultTimeoutInSeconds_WebDriver = 10;

        public async Task<HalOperationResult<T>> RequestNewWebDriverInstanceAsync<T>(SocialAccountCloudResource resource, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (resource.CloudMapServiceDiscoveryService == null)
            {
                _logger.LogError("Service discovery name is null");
                result.Failures.Add(new()
                {
                    Code = Codes.CONFIGURATION_DATA_MISSING,
                    Reason = "Required configuration data missing",
                    Detail = "AWS service discovery name missing"
                });

                return result;
            }
            // perform initial healthcheck to hal via service discovery service name
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            INewWebDriverRequest request = new NewWebDriverRequest()
            {
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                RequestUrl = RequestNewWebDriverUrl,
                ServiceDiscoveryName = resource.CloudMapServiceDiscoveryService.Name,
                DefaultTimeoutInSeconds = DefaultTimeoutInSeconds_WebDriver
            };

            return await RequestNewWebDriverAsync<T>(request, ct);
        }

        private async Task<HalOperationResult<T>> RequestNewWebDriverAsync<T>(INewWebDriverRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            _logger.LogInformation("Requesting new web driver instance from hal");
            HttpResponseMessage response = await _leadslyHalApiService.RequestNewWebDriverInstanceAsync(request, ct);

            if (response == null || response?.IsSuccessStatusCode == false)
            {
                _logger.LogError("Failed to send request to create new webdriver instance. The response is null or status code is not successful");
                result.Failures.Add(new()
                {
                    Code = Codes.HAL_API_ERROR,
                    Reason = "Failed to get a response from Hal",
                    Detail = "Failed to send request to instantiate new web driver instance"
                });
                return result;
            }

            try
            {
                _logger.LogInformation("Attempting to deserialize response.");
                string content = await response.Content.ReadAsStringAsync();
                INewWebDriverResponse newWebDriverResponse = JsonConvert.DeserializeObject<NewWebDriverResponse>(content);
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = (T)newWebDriverResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from hal");
                result.Failures.Add(new()
                {
                    Code = Codes.DESERIALIZATION_ERROR,
                    Reason = "Failed to deserialize response",
                    Detail = "Failed to deserialize response content into an object"
                });
                return result;
            }
            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ConnectUserAccountAsync<T>(SocialAccountCloudResource resource, ConnectAccountViewModel connect, string webDriverId, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (resource.CloudMapServiceDiscoveryService == null)
            {
                _logger.LogError("Service discovery name is null");
                result.Failures.Add(new()
                {
                    Code = Codes.CONFIGURATION_DATA_MISSING,
                    Reason = "Required configuration data missing",
                    Detail = "AWS service discovery name missing"
                });

                return result;
            }
            // perform initial healthcheck to hal via service discovery service name
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            IConnectAccountRequest request = new ConnectAccountRequest()
            {
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = resource.CloudMapServiceDiscoveryService.Name,
                RequestUrl = AuthenticateUserSocialAccount,
                Password = connect.Password,
                Username = connect.Username,
                WebDriverId = webDriverId,
                ConnectAuthUrl = connect.SocialAccountType == SocialAccountType.LinkedIn ? "https://www.LinkedIn.com" : throw new NotImplementedException("Url for non linkedin accounts has not been yet determined")
            };

            return await AuthenticateUserAsync<T>(request, ct);
        }

        private async Task<HalOperationResult<T>> AuthenticateUserAsync<T>(IConnectAccountRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            _logger.LogInformation("Trying to authenticate user's social account");

            HttpResponseMessage response = await _leadslyHalApiService.AuthenticateUserSocialAccountAsync(request, ct);

            if (response == null || response?.IsSuccessStatusCode == false)
            {
                _logger.LogError("Failed to send request to authenticate user's social account. The response is null or status code is not successful");
                result.Failures.Add(new()
                {
                    Code = Codes.HAL_API_ERROR,
                    Reason = "Failed to get a response from Hal",
                    Detail = "Failed to send request to authenticate user's social account"
                });
                return result;
            }

            try
            {
                _logger.LogInformation("Attempting to deserialize response.");
                string content = await response.Content.ReadAsStringAsync();
                IConnectAccountResponse connectAccountResponse = JsonConvert.DeserializeObject<ConnectAccountResponse>(content);
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = (T)connectAccountResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from hal");
                result.Failures.Add(new()
                {
                    Code = Codes.DESERIALIZATION_ERROR,
                    Reason = "Failed to deserialize response",
                    Detail = "Failed to deserialize response content into an object"
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> EnterTwoFactorAuthAsync<T>(SocialAccountCloudResource resource, TwoFactorAuthViewModel twoFactorAuth, string webDriverId, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (resource.CloudMapServiceDiscoveryService == null)
            {
                _logger.LogError("Service discovery name is null");
                result.Failures.Add(new()
                {
                    Code = Codes.CONFIGURATION_DATA_MISSING,
                    Reason = "Required configuration data missing",
                    Detail = "AWS service discovery name missing"
                });

                return result;
            }

            // perform initial healthcheck to hal via service discovery service name
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            IEnterTwoFactorAuthCodeRequest request = new EnterTwoFactorAuthCodeRequest()
            {
                RequestUrl = AuthenticateUserSocialAccount2Fa,
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = resource.CloudMapServiceDiscoveryService.Name,
                Code = twoFactorAuth.Code,
                WebDriverId = webDriverId
            };

            return await TwoFactorAuthCodeAsync<T>(request, ct);
        }

        private async Task<HalOperationResult<T>> TwoFactorAuthCodeAsync<T>(IEnterTwoFactorAuthCodeRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            _logger.LogInformation("Attempting to enter in two factor auth code");

            HttpResponseMessage response = await _leadslyHalApiService.EnterTwoFactorAuthCodeAsync(request, ct);

            if (response == null || response?.IsSuccessStatusCode == false)
            {
                _logger.LogError("Failed to send request to enter user's two factor auth code. The response is null or status code is not successful");
                result.Failures.Add(new()
                {
                    Code = Codes.HAL_API_ERROR,
                    Reason = "Failed to get a response from Hal",
                    Detail = "Failed to send request to authenticate user's social account"
                });
                return result;
            }

            try
            {
                _logger.LogInformation("Attempting to deserialize response.");
                string content = await response.Content.ReadAsStringAsync();
                IEnterTwoFactorAuthCodeResponse connectAccountResponse = JsonConvert.DeserializeObject<EnterTwoFactorAuthCodeResponse>(content);
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = (T)connectAccountResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from hal");
                result.Failures.Add(new()
                {
                    Code = Codes.DESERIALIZATION_ERROR,
                    Reason = "Failed to deserialize response",
                    Detail = "Failed to deserialize response content into an object"
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
