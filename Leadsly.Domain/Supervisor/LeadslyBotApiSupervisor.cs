using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
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
    }
}
