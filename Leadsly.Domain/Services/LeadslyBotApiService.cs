using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
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
        public LeadslyBotApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;
        private static Serilog.ILogger _logger => Serilog.Log.ForContext<LeadslyBotApiService>();

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
    }
}
