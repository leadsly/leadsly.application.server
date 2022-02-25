using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Leadsly.Models.Entities;
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
        Task<InstantiateNewWebDriverResult> RequestNewWebDriverInstanceAsync(SocialAccountCloudResource cloudResource, CancellationToken ct = default);
        Task<ConnectUserAccountResult> ConnectUserAccountAsync(SocialAccountCloudResource resource, ConnectAccountViewModel connect, string webDriverId, CancellationToken ct = default);
        Task<EnterTwoFactorAuthResults> EnterTwoFactorAuthAsync(SocialAccountCloudResource resource, TwoFactorAuthViewModel twoFactorAuth, string webDriverId, CancellationToken ct = default);
    }
}
