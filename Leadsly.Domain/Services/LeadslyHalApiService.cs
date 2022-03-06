using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Application.Model.Requests.Hal;

namespace Leadsly.Domain.Services
{
    public class LeadslyHalApiService : ILeadslyHalApiService
    {
        public LeadslyHalApiService(HttpClient httpClient, ILogger<LeadslyHalApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<LeadslyHalApiService> _logger;
        private const string HttpPrefix = "http://";

        public async Task<HttpResponseMessage> PerformHealthCheckAsync(HealthCheckRequest halRequest, CancellationToken ct = default)
        {
            string url = halRequest.PrivateIpAddress != null ? $"{HttpPrefix}{halRequest.PrivateIpAddress}" : $"{HttpPrefix}{halRequest.ServiceDiscoveryName}.{halRequest.NamespaceName}";

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
            // string url = $"{HttpPrefix}{instantiateNewWebDriverRequest.ServiceDiscoveryName}.{instantiateNewWebDriverRequest.NamespaceName}";
            string url = "http://localhost:5020";
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
            // string url = $"{HttpPrefix}{authRequest.ServiceDiscoveryName}.{authRequest.NamespaceName}";
            string url = "http://localhost:5020";
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{authRequest.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    Username = authRequest.Username,
                    Password = authRequest.Password,
                    ConnectAuthUrl = authRequest.ConnectAuthUrl,
                    BrowserPurpose = authRequest.BrowserPurpose
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

        public async Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(IEnterTwoFactorAuthCodeRequest enterTwoFactorAuthRequest, CancellationToken ct = default)
        {
            // string url = $"{HttpPrefix}{enterTwoFactorAuthRequest.ServiceDiscoveryName}.{enterTwoFactorAuthRequest.NamespaceName}";
            string url = "http://localhost:5020";

            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{enterTwoFactorAuthRequest.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    WindowHandleId = enterTwoFactorAuthRequest.WindowHandleId,
                    Code = enterTwoFactorAuthRequest.Code
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
