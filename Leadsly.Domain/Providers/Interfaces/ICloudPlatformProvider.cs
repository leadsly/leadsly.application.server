using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws;
using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICloudPlatformProvider
    {
        public Task<NewSocialAccountSetupResult> SetupNewCloudResourceForUserSocialAccountAsync(string userId, CancellationToken ct = default);        
        public Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, string userId, CancellationToken ct = default);
        public Task<ExistingSocialAccountSetupResultDTO> ConnectToExistingCloudResourceAsync(SocialAccount socialAccount, CancellationToken ct = default);
        public Task RemoveUsersSocialAccountCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default);
    }
}
