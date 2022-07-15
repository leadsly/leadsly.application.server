using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IMonitorForNewConnectionsPhaseRepository
    {
        Task<IList<MonitorForNewConnectionsPhase>> GetAllByUserIdAsync(string userId, CancellationToken ct = default);
        Task<MonitorForNewConnectionsPhase> GetBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default);
        Task<MonitorForNewConnectionsPhase> CreateAsync(MonitorForNewConnectionsPhase phase, CancellationToken ct = default);
    }
}
