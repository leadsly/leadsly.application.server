using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Models.Responses;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ILeadslyHalProvider
    {
        Task<HalOperationResult<T>> RequestNewWebDriverInstanceAsync<T>(SocialAccountCloudResource cloudResource, CancellationToken ct = default)
            where T : IOperationResponse;
        Task<HalOperationResult<T>> ConnectUserAccountAsync<T>(SocialAccountCloudResource resource, Leadsly.Application.Model.Requests.ConnectAccountRequest connect, CancellationToken ct = default)
            where T : IOperationResponse;
        Task<HalOperationResult<T>> EnterTwoFactorAuthAsync<T>(SocialAccountCloudResource resource, Leadsly.Application.Model.Requests.TwoFactorAuthRequest twoFactorAuth, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<EnterTwoFactorAuthResponse> EnterTwoFactorAuthAsync(string code, string resourceDiscoveryName, CancellationToken ct = default);
        Task<HalUnit> GetHalDetailsByConnectedAccountUsernameAsync(string connectedAccountUsername, CancellationToken ct = default);

        Task<ConnectLinkedInAccountResponse> ConnectAccountAsync(string email, string password, string resourceDiscoveryServiceName, IHeaderDictionary responseHeaders, IHeaderDictionary requestHeaders, CancellationToken ct = default);
    }
}
