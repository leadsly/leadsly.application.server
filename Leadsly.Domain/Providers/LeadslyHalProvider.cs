﻿using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class LeadslyHalProvider : ILeadslyHalProvider
    {
        public LeadslyHalProvider(
            ILeadslyHalApiService leadslyHalApiService,
            ICloudPlatformRepository cloudPlatformRepository,
            IHalRepository halRepository,
            ILogger<LeadslyHalProvider> logger)
        {
            _leadslyHalApiService = leadslyHalApiService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _logger = logger;
            _halRepository = halRepository;
        }

        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILeadslyHalApiService _leadslyHalApiService;
        private readonly IHalRepository _halRepository;
        private readonly ILogger<LeadslyHalProvider> _logger;

        public async Task<EnterTwoFactorAuthResponse> EnterTwoFactorAuthAsync(string code, string resourceDiscoveryName, string gridDiscoveryServiceName, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            EnterTwoFactorAuthRequest request = new()
            {
                GridNamespaceName = configuration.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridDiscoveryServiceName,
                RequestUrl = "linkedin/2fa",
                NamespaceName = configuration.ServiceDiscoveryConfig.Hal.Name,
                ServiceDiscoveryName = resourceDiscoveryName,
                Code = code
            };

            HttpResponseMessage resp = await _leadslyHalApiService.EnterTwoFactorAuthCodeAsync(request, ct);
            if (resp == null || resp.StatusCode == HttpStatusCode.InternalServerError || resp.IsSuccessStatusCode == false)
            {
                IEnumerable<string> values = default;
                resp?.Headers.TryGetValues(CustomHeaderKeys.Origin, out values);
                _logger.LogError($"Failed to send request to enter two factor auth code for hal id {values?.FirstOrDefault()}. The response is null or status code is not successful");
                return null;
            }

            string content = await resp.Content?.ReadAsStringAsync();
            EnterTwoFactorAuthResponse response = JsonConvert.DeserializeObject<EnterTwoFactorAuthResponse>(content);
            return response;
        }

        public async Task<EnterEmailChallengePinResponse> EnterEmailChallengePinAsync(string pin, string resourceDiscoveryName, string gridDiscoveryServiceName, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            EnterEmailChallengePinRequest request = new()
            {
                GridNamespaceName = configuration.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridDiscoveryServiceName,
                RequestUrl = "linkedin/email-challenge-pin",
                NamespaceName = configuration.ServiceDiscoveryConfig.Hal.Name,
                ServiceDiscoveryName = resourceDiscoveryName,
                Pin = pin
            };

            HttpResponseMessage resp = await _leadslyHalApiService.EnterEmailChallengePinAsync(request, ct);
            if (resp == null || resp.StatusCode == HttpStatusCode.InternalServerError || resp.IsSuccessStatusCode == false)
            {
                IEnumerable<string> values = default;
                resp?.Headers.TryGetValues(CustomHeaderKeys.Origin, out values);
                _logger.LogError($"Failed to send request to enter email challenge pin for hal id {values?.FirstOrDefault()}. The response is null or status code is not successful");
                return null;
            }
            _logger.LogInformation("Successfully sent request to Hal to enter email challenge pin");

            string content = await resp.Content?.ReadAsStringAsync();
            EnterEmailChallengePinResponse response = JsonConvert.DeserializeObject<EnterEmailChallengePinResponse>(content);
            return response;
        }

        public async Task<ConnectLinkedInAccountResponse> ConnectAccountAsync(string email, string password, string resourceDiscoveryServiceName, string gridDiscoveryServiceName, string proxyDiscoveryServiceName, IHeaderDictionary responseHeaders, IHeaderDictionary requestHeaders, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            AuthenticateLinkedInAccountRequest request = new()
            {
                GridNamespaceName = configuration.ServiceDiscoveryConfig.Grid.Name,
                GridServiceDiscoveryName = gridDiscoveryServiceName,
                ProxyNamespaceName = configuration.ServiceDiscoveryConfig.Proxy.Name,
                ProxyServiceDiscoveryName = proxyDiscoveryServiceName,
                NamespaceName = configuration.ServiceDiscoveryConfig.Hal.Name,
                ServiceDiscoveryName = resourceDiscoveryServiceName,
                RequestUrl = "linkedin/signin",
                Password = password,
                Username = email,
                AttemptNumber = 1
            };

            HttpResponseMessage resp = await SignInUserAsync(request, requestHeaders, ct);
            HttpStatusCode statusCode = resp.StatusCode;
            _logger.LogDebug("Connect account response from hal had status code of: {statusCode}", statusCode.ToString());

            string content = string.Empty;
            if (resp != null)
            {
                content = await resp.Content?.ReadAsStringAsync();
            }
            else
            {
                _logger.LogError("Failed to connect account to hal. The response is null");
                return null;
            }

            ConnectLinkedInAccountResponse response = JsonConvert.DeserializeObject<ConnectLinkedInAccountResponse>(content);

            resp.Headers.TryGetValues("X-Auth-Attempt-Count", out IEnumerable<string> attemptCount);
            if (attemptCount.Count() > 0)
            {
                responseHeaders.Add("X-Auth-Attempt-Count", attemptCount.First());
            }

            return response;
        }

        private async Task<HttpResponseMessage> SignInUserAsync(AuthenticateLinkedInAccountRequest request, IHeaderDictionary requestHeaders, CancellationToken ct = default)
        {
            HttpResponseMessage response = await _leadslyHalApiService.SignInAsync(request, requestHeaders, ct);

            if (response == null || response.StatusCode == HttpStatusCode.InternalServerError || response.IsSuccessStatusCode == false)
            {
                IEnumerable<string> values = default;
                response?.Headers.TryGetValues(CustomHeaderKeys.Origin, out values);
                _logger.LogError($"Failed to send request to sign in user for hal id {values?.FirstOrDefault()}. The response is null or status code is not successful");
                return null;
            }

            return response;
        }
    }
}
