using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Leadsly.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<LeadslyConnectionResult> ConnectAccountToLeadslyAsync(ConnectLeadslyViewModel connectAccount, CancellationToken ct = default)
        {
            await _leadslyBotApiService.ConnectToLeadslyAsync(connectAccount, ct);

            LeadslyConnectionResult result = new LeadslyConnectionResult();

            return result;
        }

        public async Task<LeadslySetupResultDTO> SetupLeadslyForUserAsync(LeadslySetupDTO setup, CancellationToken ct = default)
        {
            List<DockerContainerInfo> usersContainers = await _containerRepository.GetContainersByUserId(setup.UserId);

            if(usersContainers == null || usersContainers.Count == 0)
            {
                var res = await _awsElasticContainerProvider.SetupUsersContainer();
            }

            // if user has multiple containers figure out how to determine which container to use

            return new LeadslySetupResultDTO { };

        }
    }
}
