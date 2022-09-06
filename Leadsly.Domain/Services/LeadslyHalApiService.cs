using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
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

        public async Task<HttpResponseMessage> SignInAsync(AuthenticateLinkedInAccountRequest request, IHeaderDictionary requestHeaders, CancellationToken ct = default)
        {
            string url = _urlService.GetHalsBaseUrl(request.NamespaceName, request.ServiceDiscoveryName);
            HttpRequestMessage req = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{request.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    GridNamespaceName = request.GridNamespaceName,
                    GridServiceDiscoveryName = request.GridServiceDiscoveryName,
                    Username = request.Username,
                    Password = request.Password,
                    ConnectAuthUrl = request.ConnectAuthUrl,
                    BrowserPurpose = request.BrowserPurpose,
                    AttemptNumber = request.AttemptNumber
                })
            };

            if (requestHeaders.ContainsKey("X-Auth-Attempt-Count") == true)
            {
                requestHeaders.TryGetValue("X-Auth-Attempt-Count", out StringValues attemptCount);
                string headerValue = attemptCount.ToString();
                req.Headers.Add("X-Auth-Attempt-Count", headerValue);
            }

            if (requestHeaders.ContainsKey("Authorization"))
            {
                string access_token = requestHeaders["Authorization"];
                req.Headers.Add("Authorization", access_token);
            }

            HttpResponseMessage response = default;
            try
            {
                string requestUri = request.RequestUrl;
                _logger.LogInformation("Request has been sent to sign user in base url {url} and full path {requestUri}", url, requestUri);
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to sign user into their linked in account");
            }

            return response;
        }

        public async Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(EnterTwoFactorAuthRequest request, CancellationToken ct = default)
        {
            string url = _urlService.GetHalsBaseUrl(request.NamespaceName, request.ServiceDiscoveryName);

            HttpRequestMessage req = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{request.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    GridNamespaceName = request.GridNamespaceName,
                    GridServiceDiscoveryName = request.GridServiceDiscoveryName,
                    WindowHandleId = request.WindowHandleId,
                    Code = request.Code,
                    BrowserPurpose = request.BrowserPurpose,
                    AttemptNumber = request.AttemptNumber
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Request has been sent to enter user's two factor auth code", url);
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to enter user's two factor auth code");
            }

            return response;
        }

        public async Task<HttpResponseMessage> EnterEmailChallengePinAsync(EnterEmailChallengePinRequest request, CancellationToken ct = default)
        {
            string url = _urlService.GetHalsBaseUrl(request.NamespaceName, request.ServiceDiscoveryName);

            HttpRequestMessage req = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{request.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    GridNamespaceName = request.GridNamespaceName,
                    GridServiceDiscoveryName = request.GridServiceDiscoveryName,
                    WindowHandleId = request.WindowHandleId,
                    Pin = request.Pin,
                    BrowserPurpose = request.BrowserPurpose,
                    AttemptNumber = request.AttemptNumber
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Request has been sent to enter email challenge pin", url);
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to enter email challenge pin");
            }

            return response;
        }
    }
}
