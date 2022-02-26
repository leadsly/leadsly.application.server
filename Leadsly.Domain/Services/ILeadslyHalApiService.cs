using Leadsly.Domain.Models;
using Leadsly.Models.ViewModels.Hal;
using Leadsly.Models;
using Leadsly.Models.Requests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Models.Requests.Hal;

namespace Leadsly.Domain.Services
{
    public interface ILeadslyHalApiService
    {
        // Task<HttpResponseMessage> RequestNewWebDriverInstanceAsync(NewWebDriverRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> RequestNewWebDriverInstanceAsync(INewWebDriverRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> PerformHealthCheckAsync(HalRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> AuthenticateUserSocialAccountAsync(IConnectAccountRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(IEnterTwoFactorAuthCodeRequest request, CancellationToken ct = default);
    }
}
