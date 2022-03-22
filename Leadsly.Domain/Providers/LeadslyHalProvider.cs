using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Requests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Application.Model.Requests.Hal;
using ConnectAccountRequest = Leadsly.Application.Model.Requests.Hal.ConnectAccountRequest;
using NewWebDriverRequest = Leadsly.Application.Model.Requests.Hal.NewWebDriverRequest;
using Leadsly.Domain.Converters;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.Responses;
using Microsoft.AspNetCore.Mvc;
using Leadsly.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using System.Net.Http.Formatting;
using Leadsly.Domain.Services.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.Requests.Hal.Interfaces;

namespace Leadsly.Domain.Providers
{
    public class LeadslyHalProvider : ILeadslyHalProvider
    {
        public LeadslyHalProvider(ILeadslyHalApiService leadslyHalApiService, ICloudPlatformRepository cloudPlatformRepository, ISerializerFacade deserializerFacade, ILogger<LeadslyHalProvider> logger)
        {
            _leadslyHalApiService = leadslyHalApiService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _deserializerFacade = deserializerFacade;
            _logger = logger;
        }

        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILeadslyHalApiService _leadslyHalApiService;
        private readonly ISerializerFacade _deserializerFacade;
        private readonly ILogger<LeadslyHalProvider> _logger;
        private readonly string RequestNewWebDriverUrl = "api/webdriver";
        private readonly string AuthenticateUserSocialAccount = "api/authentication";
        private readonly string AuthenticateUserSocialAccount2Fa = "api/authentication/2fa";
        // zero is default and it's best to keep it that way.
        private readonly int DefaultTimeoutInSeconds_WebDriver = 0;

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

            if (response == null || response?.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                _logger.LogError("Failed to send request to create new webdriver instance. The response is null or status code is not successful");
                IEnumerable<string> values = default;
                response?.Headers.TryGetValues(CustomHeaderKeys.Origin, out values);
                result.Failures.Add(new()
                {
                    Code = Codes.HAL_INTERNAL_SERVER_ERROR,
                    Reason = response?.StatusCode == System.Net.HttpStatusCode.InternalServerError ? await response.Content.ReadAsStringAsync() : "Response to get new webdriver is null",
                    Detail = $"Failed to send new web driver request to hal {values?.FirstOrDefault()}",
                });                
                return result;
            }

            if(response.IsSuccessStatusCode == false)
            {
                return await HandleFailedHalResponseAsync<T>(response);
            }

            result = await DeserializeNewWebDriverResponse<T>(response);
            
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private async Task<HalOperationResult<T>> DeserializeNewWebDriverResponse<T>(HttpResponseMessage response)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            try
            {
                _logger.LogInformation("Attempting to deserialize response.");
                string content = await response.Content.ReadAsStringAsync();
                INewWebDriverResponse resp = JsonConvert.DeserializeObject<NewWebDriverResponse>(content);
                _logger.LogInformation("Successfully deserialized response into an object");
                result.Value = (T)resp;
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

        public async Task<HalOperationResult<T>> ConnectUserAccountAsync<T>(SocialAccountCloudResource resource, Leadsly.Application.Model.Requests.ConnectAccountRequest connect, CancellationToken ct = default)
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
                BrowserPurpose = connect.BrowserPurpose,
                AttemptNumber = connect.AttemptNumber,
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

            if (response == null || response?.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {                
                _logger.LogError("Failed to send request to authenticate user's social account. The response is null or status code is not successful");
                IEnumerable<string> values = default;
                response?.Headers.TryGetValues(CustomHeaderKeys.Origin, out values);
                result.Failures.Add(new()
                {
                    Code = Codes.HAL_INTERNAL_SERVER_ERROR,
                    Reason = response?.StatusCode == System.Net.HttpStatusCode.InternalServerError ? await response.Content.ReadAsStringAsync() : "Response to authenticate user's social account is null",
                    Detail = $"Failed to send authentication request to hal {values?.FirstOrDefault()}",
                });
                return result;
            }

            if(response.IsSuccessStatusCode == false)
            {                
                return await HandleFailedHalResponseAsync<T>(response);
            }

            result = await _deserializerFacade.DeserializeConnectAccountResponseAsync<T>(response);

            // check if deserialization succeeded
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private async Task<HalOperationResult<T>> HandleFailedHalResponseAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                // special handling for validation and problem details because extension properties were not deserializing
                result.ProblemDetails = await response.Content.ReadAsAsync<ValidationProblemDetails>(
                    new[] {
                        new JsonMediaTypeFormatter
                        {
                            SerializerSettings = new JsonSerializerSettings
                            {
                                Converters =  { new ValidationProblemDetailsConverter(), new ProblemDetailsConverter() }
                            }
                        }
                    });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize problem details response");
            }

            return result;
        }

        public async Task<HalOperationResult<T>> EnterTwoFactorAuthAsync<T>(SocialAccountCloudResource resource, TwoFactorAuthRequest twoFactorAuth, CancellationToken ct = default)
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
                WindowHandleId = twoFactorAuth.WindowHandleId,
                BrowserPurpose = twoFactorAuth.BrowserPurpose,
                AttemptNumber = twoFactorAuth.AttemptNumber
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

            if (response == null || response?.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                _logger.LogError("Failed to send request to enter user's two factor auth code. The response is null or status code is not successful");
                IEnumerable<string> values = default;
                response?.Headers.TryGetValues(CustomHeaderKeys.Origin, out values);
                result.Failures.Add(new()
                {
                    Code = Codes.HAL_INTERNAL_SERVER_ERROR,
                    Reason = response?.StatusCode == System.Net.HttpStatusCode.InternalServerError ? await response.Content.ReadAsStringAsync() : "Response to enter user's two factor auth code null",
                    Detail = $"Failed to send two factor auth code request to hal {values?.FirstOrDefault()}",
                });
                return result;
            }

            if(response.IsSuccessStatusCode == false)
            {
                return await HandleFailedHalResponseAsync<T>(response);
            }

            result = await _deserializerFacade.DeserializeEnterTwoFactorAuthCodeResponseAsync<T>(response);
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        
    }
}
