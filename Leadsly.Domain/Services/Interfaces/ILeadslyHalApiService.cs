using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;
using Leadsly.Domain.Models.Requests;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ILeadslyHalApiService
    {
        Task<HttpResponseMessage> RequestNewWebDriverInstanceAsync(INewWebDriverRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> PerformHealthCheckAsync(HealthCheckRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> AuthenticateUserSocialAccountAsync(IConnectAccountRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> SignInAsync(AuthenticateLinkedInAccountRequest request, IHeaderDictionary requestHeaders, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(IEnterTwoFactorAuthCodeRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(EnterTwoFactorAuthRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterEmailChallengePinAsync(EnterEmailChallengePinRequest request, CancellationToken ct = default);
    }
}
