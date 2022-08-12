using Leadsly.Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ILeadslyHalProvider
    {
        Task<EnterTwoFactorAuthResponse> EnterTwoFactorAuthAsync(string code, string resourceDiscoveryName, string gridDiscoveryServiceName, CancellationToken ct = default);
        Task<ConnectLinkedInAccountResponse> ConnectAccountAsync(string email, string password, string resourceDiscoveryServiceName, string gridDiscoveryServiceName, IHeaderDictionary responseHeaders, IHeaderDictionary requestHeaders, CancellationToken ct = default);
    }
}
