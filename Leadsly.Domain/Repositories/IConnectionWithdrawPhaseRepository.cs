using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IConnectionWithdrawPhaseRepository
    {
        Task<ConnectionWithdrawPhase> CreateAsync(ConnectionWithdrawPhase phase, CancellationToken ct = default);
    }
}
