using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public interface ILeadslyBotApiService
    {
        Task<LeadslyConnectionResult> ConnectToLeadslyAsync(ConnectLeadslyViewModel connectLeadsly, CancellationToken ct = default);
        Task<HttpResponseMessage> PerformHealthCheckAsync(HalRequest request, CancellationToken ct = default);
    }
}
