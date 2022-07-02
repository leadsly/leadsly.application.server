using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;
using Leadsly.Domain.Models.Requests;
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
        Task<HttpResponseMessage> SignInAsync(AuthenticateLinkedInAccountRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(IEnterTwoFactorAuthCodeRequest request, CancellationToken ct = default);
    }
}
