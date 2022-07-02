using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class LeadslyHalApiService : ILeadslyHalApiService
    {
        public LeadslyHalApiService(HttpClient httpClient, ILogger<LeadslyHalApiService> logger, IUrlService urlService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _urlService = urlService;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<LeadslyHalApiService> _logger;
        private readonly IUrlService _urlService;

        public async Task<HttpResponseMessage> PerformHealthCheckAsync(HealthCheckRequest halRequest, CancellationToken ct = default)
        {
            string halUrl = _urlService.GetHalsBaseUrl(halRequest.NamespaceName);
            string url = halRequest.PrivateIpAddress != null ? $"https://{halRequest.PrivateIpAddress}" : halUrl;

            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{url}/{halRequest.RequestUrl}", UriKind.Absolute)
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Performing health check to {url}", url);
                response = await _httpClient.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get healthcheck on hal");
            }

            return response;
        }

        public async Task<HttpResponseMessage> RequestNewWebDriverInstanceAsync(INewWebDriverRequest instantiateNewWebDriverRequest, CancellationToken ct = default)
        {
            string url = _urlService.GetHalsBaseUrl(instantiateNewWebDriverRequest.NamespaceName);
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{instantiateNewWebDriverRequest.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    DefaultTimeoutInSeconds = instantiateNewWebDriverRequest.DefaultTimeoutInSeconds
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Request has been sent to instnatiate a new webdriver instance", url);
                response = await _httpClient.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to instantiate new web driver instance");
            }

            return response;
        }

        public async Task<HttpResponseMessage> AuthenticateUserSocialAccountAsync(IConnectAccountRequest authRequest, CancellationToken ct = default)
        {
            string url = _urlService.GetHalsBaseUrl(authRequest.NamespaceName);
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{authRequest.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    Username = authRequest.Username,
                    Password = authRequest.Password,
                    ConnectAuthUrl = authRequest.ConnectAuthUrl,
                    BrowserPurpose = authRequest.BrowserPurpose,
                    AttemptNumber = authRequest.AttemptNumber
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Request has been sent to authenticate user's account", url);
                response = await _httpClient.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to instantiate new web driver instance");
            }

            return response;
        }

        public async Task<HttpResponseMessage> SignInAsync(AuthenticateLinkedInAccountRequest request, CancellationToken ct = default)
        {
            string url = _urlService.GetHalsBaseUrl(request.NamespaceName);
            HttpRequestMessage req = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{request.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    Username = request.Username,
                    Password = request.Password,
                    ConnectAuthUrl = request.ConnectAuthUrl,
                    BrowserPurpose = request.BrowserPurpose,
                    AttemptNumber = request.AttemptNumber
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Request has been sent to sign user in", url);
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to sign user into their linked in account");
            }

            return response;
        }

        public async Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(IEnterTwoFactorAuthCodeRequest enterTwoFactorAuthRequest, CancellationToken ct = default)
        {
            string url = _urlService.GetHalsBaseUrl(enterTwoFactorAuthRequest.NamespaceName);

            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{enterTwoFactorAuthRequest.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    WindowHandleId = enterTwoFactorAuthRequest.WindowHandleId,
                    Code = enterTwoFactorAuthRequest.Code,
                    BrowserPurpose = enterTwoFactorAuthRequest.BrowserPurpose,
                    AttemptNumber = enterTwoFactorAuthRequest.AttemptNumber
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Request has been sent to enter user's two factor auth code", url);
                response = await _httpClient.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to enter user's two factor auth code");
            }

            return response;
        }
    }
}
