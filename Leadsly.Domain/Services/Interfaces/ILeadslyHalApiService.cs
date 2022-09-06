using Leadsly.Domain.Models.Requests;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ILeadslyHalApiService
    {
        Task<HttpResponseMessage> SignInAsync(AuthenticateLinkedInAccountRequest request, IHeaderDictionary requestHeaders, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterTwoFactorAuthCodeAsync(EnterTwoFactorAuthRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> EnterEmailChallengePinAsync(EnterEmailChallengePinRequest request, CancellationToken ct = default);
    }
}
