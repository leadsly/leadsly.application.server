using Leadsly.Models;
using Leadsly.Models.Aws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public interface IAwsElasticContainerProvider
    {
        public Task<AwsOperationResult> SetupNewUsersContainerAsync(SetupNewUserInLeadslyDTO createContainer, CancellationToken ct = default);

        public Task<AwsOperationResult> SetupExistingContainerAsync(SetupExistingUserInLeadslyDTO existingContainer, CancellationToken ct = default);
    }
}
