using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ILeadslyHalApiService
    {        
        Task<HttpResponseMessage> RequestNewWebDriverInstanceAsync(INewWebDriverRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> PerformHealthCheckAsync(HealthCheckRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> AuthenticateUserSocialAccountAsync(IConnectAccountRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(IEnterTwoFactorAuthCodeRequest request, CancellationToken ct = default);
    }
}
