using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels.Response;
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
        Task<HalOperationResult<T>> ConnectUserAccountAsync<T>(SocialAccountCloudResource resource, Leadsly.Application.Model.Requests.ConnectAccountRequest connect, CancellationToken ct = default)
            where T : IOperationResponse;        
        Task<HalOperationResult<T>> EnterTwoFactorAuthAsync<T>(SocialAccountCloudResource resource, Leadsly.Application.Model.Requests.TwoFactorAuthRequest twoFactorAuth, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
