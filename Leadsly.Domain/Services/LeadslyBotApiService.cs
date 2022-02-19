using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class LeadslyBotApiService : ILeadslyBotApiService
    {
        public LeadslyBotApiService(HttpClient httpClient, ILogger<LeadslyBotApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<LeadslyBotApiService> _logger;
        private const string HttpPrefix = "http://";
        public async Task<LeadslyConnectionResult> ConnectToLeadslyAsync(ConnectLeadslyViewModel connectLeadsly, CancellationToken ct = default)
        {
            LeadslyConnectionResult result = new();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("")
            };
            HttpResponseMessage response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode == false)
            {
                result.Succeeded = false;
                return result;
            }

            return result;
        }

        public async Task<HttpResponseMessage> PerformHealthCheckAsync(HalRequest halRequest, CancellationToken ct = default)
        {
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(halRequest.RequestUrl, UriKind.Relative)
            };

            HttpResponseMessage response = default;           
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    Uri baseAddress = halRequest.PrivateIpAddress != null ? new Uri($"{HttpPrefix}{halRequest.PrivateIpAddress}", UriKind.Absolute) : new Uri($"{HttpPrefix}{halRequest.DiscoveryServiceName}.{halRequest.NamespaceName}", UriKind.Absolute);
                    string url = baseAddress + request.RequestUri.ToString();                    
                    _logger.LogInformation("Performing health check to {url}", url);                    
                    httpClient.BaseAddress = baseAddress;
                    response = await httpClient.SendAsync(request, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get healthcheck on hal");
            }

            return response;
        }
    }
}
