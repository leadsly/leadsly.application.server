using Leadsly.Models;
using Leadsly.Models.Entities;
using Leadsly.Models.Responses;
using Leadsly.Models.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public interface ILeadslyHalProvider
    {
        Task<HalOperationResult<T>> RequestNewWebDriverInstanceAsync<T>(SocialAccountCloudResource cloudResource, CancellationToken ct = default)
            where T : IOperationResponse;        
        Task<HalOperationResult<T>> ConnectUserAccountAsync<T>(SocialAccountCloudResource resource, Leadsly.Models.Requests.ConnectAccountRequest connect, string webDriverId, CancellationToken ct = default)
            where T : IOperationResponse;        
        Task<HalOperationResult<T>> EnterTwoFactorAuthAsync<T>(SocialAccountCloudResource resource, Leadsly.Models.Requests.TwoFactorAuthRequest twoFactorAuth, string webDriverId, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
