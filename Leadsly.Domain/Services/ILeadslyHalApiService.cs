using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Leadsly.Models.Requests;
using Leadsly.Models.Respones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public interface ILeadslyHalApiService
    {
        Task<HttpResponseMessage> RequestNewWebDriverInstanceAsync(IntantiateNewWebDriverRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> PerformHealthCheckAsync(HalRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> AuthenticateUserSocialAccountAsync(ConnectUserAccountRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(EnterTwoFactorAuthCodeRequest request, CancellationToken ct = default);
    }
}
