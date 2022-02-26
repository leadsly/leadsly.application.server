using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Leadsly.Models.Entities;
using Leadsly.Models.Requests;
using Leadsly.Models.Respones;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<InstantiateNewWebDriverResult> RequestNewWebDriverInstanceAsync(SocialAccountCloudResource resource, CancellationToken ct = default)
        {
            InstantiateNewWebDriverResult result = new()
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
            IntantiateNewWebDriverRequest request = new()
            {
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                RequestUrl = RequestNewWebDriverUrl,
                ServiceDiscoveryName = resource.CloudMapServiceDiscoveryService.Name,
                DefaultTimeoutInSeconds = DefaultTimeoutInSeconds_WebDriver
            };

            return await RequestNewWebDriverAsync(request, ct);
        }

        private async Task<InstantiateNewWebDriverResult> RequestNewWebDriverAsync(IntantiateNewWebDriverRequest request, CancellationToken ct = default)
        {
            InstantiateNewWebDriverResult result = new()
            {
                Succeeded = false
            };

            _logger.LogInformation("Requesting new web driver instance from hal");
            HttpResponseMessage response = await _leadslyHalApiService.RequestNewWebDriverInstanceAsync(request, ct);

            if(response == null || response?.IsSuccessStatusCode == false)
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
                IntantiateNewWebDriverResponse newWebDriverResponse = JsonConvert.DeserializeObject<IntantiateNewWebDriverResponse>(content);
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = newWebDriverResponse;
            }
            catch(Exception ex)
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

        public async Task<ConnectUserAccountResult> ConnectUserAccountAsync(SocialAccountCloudResource resource, ConnectAccountViewModel connect, string webDriverId, CancellationToken ct = default)
        {
            ConnectUserAccountResult result = new()
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
            ConnectUserAccountRequest request = new()
            {
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = resource.CloudMapServiceDiscoveryService.Name,
                RequestUrl = AuthenticateUserSocialAccount,
                Password = connect.Password,
                Username = connect.Username,
                WebDriverId = webDriverId,
                ConnectAuthUrl = connect.SocialAccountType == SocialAccountType.LinkedIn ? "https://www.LinkedIn.com" : throw new NotImplementedException("Url for non linkedin accounts has not been yet determined")
            };

            return await AuthenticateUserAsync(request, ct);            
        }

        private async Task<ConnectUserAccountResult> AuthenticateUserAsync(ConnectUserAccountRequest request, CancellationToken ct = default)
        {
            ConnectUserAccountResult result = new()
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
                ConnectUserAccountResponse connectAccountResponse = JsonConvert.DeserializeObject<ConnectUserAccountResponse>(content);
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = connectAccountResponse;
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

        public async Task<EnterTwoFactorAuthResults> EnterTwoFactorAuthAsync(SocialAccountCloudResource resource, TwoFactorAuthViewModel twoFactorAuth, string webDriverId, CancellationToken ct = default)
        {
            EnterTwoFactorAuthResults result = new()
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

            EnterTwoFactorAuthCodeRequest request = new()
            {
                RequestUrl = AuthenticateUserSocialAccount2Fa,
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                ServiceDiscoveryName = resource.CloudMapServiceDiscoveryService.Name,
                TwoFactorAuthCode = twoFactorAuth.Code,
                WebDriverId = webDriverId
            };

            return await TwoFactorAuthCodeAsync(request, ct);
        }

        private async Task<EnterTwoFactorAuthResults> TwoFactorAuthCodeAsync(EnterTwoFactorAuthCodeRequest request, CancellationToken ct = default)
        {
            EnterTwoFactorAuthResults result = new()
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
                EnterTwoFactorAuthCodeResponse connectAccountResponse = JsonConvert.DeserializeObject<EnterTwoFactorAuthCodeResponse>(content);
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = connectAccountResponse;
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
