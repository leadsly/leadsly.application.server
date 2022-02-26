using Leadsly.Models.ViewModels.Hal;
using Leadsly.Models;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Models.ViewModels.Interfaces;

namespace Leadsly.Domain.Providers
{
    public interface ILeadslyHalProvider
    {
        Task<HalOperationResult<T>> RequestNewWebDriverInstanceAsync<T>(SocialAccountCloudResource cloudResource, CancellationToken ct = default)
            where T : IOperationResponse;
        // Task<ConnectUserAccountResult> ConnectUserAccountAsync(SocialAccountCloudResource resource, ConnectAccountViewModel connect, string webDriverId, CancellationToken ct = default);
        Task<HalOperationResult<T>> ConnectUserAccountAsync<T>(SocialAccountCloudResource resource, ConnectAccountViewModel connect, string webDriverId, CancellationToken ct = default)
            where T : IOperationResponse;
        //Task<EnterTwoFactorAuthResults> EnterTwoFactorAuthAsync(SocialAccountCloudResource resource, TwoFactorAuthViewModel twoFactorAuth, string webDriverId, CancellationToken ct = default);
        Task<HalOperationResult<T>> EnterTwoFactorAuthAsync<T>(SocialAccountCloudResource resource, TwoFactorAuthViewModel twoFactorAuth, string webDriverId, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
