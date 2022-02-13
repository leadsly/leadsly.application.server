using Leadsly.Models;
using Leadsly.Models.Aws;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public interface ICloudPlatformProvider
    {
        public Task<NewSocialAccountSetupResult> SetupNewContainerForUserSocialAccountAsync(CancellationToken ct = default);        
        public Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, CancellationToken ct = default);
        public Task<CloudPlatformOperationResult> SetupExistingContainerAsync(SetupExistingUserInLeadslyDTO existingContainer, CancellationToken ct = default);
    }
}
