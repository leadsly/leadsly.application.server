using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IConnectionWithdrawPhaseRepository
    {
        Task<ConnectionWithdrawPhase> CreateAsync(ConnectionWithdrawPhase phase, CancellationToken ct = default);
    }
}
