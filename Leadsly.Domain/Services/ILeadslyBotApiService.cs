using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public interface ILeadslyBotApiService
    {
        Task<LeadslyConnectionResult> ConnectToLeadslyAsync(ConnectLeadslyViewModel connectLeadsly, CancellationToken ct = default);
    }
}
