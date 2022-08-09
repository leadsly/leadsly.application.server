using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IScanProspectsForRepliesPhaseRepository
    {
        Task<ScanProspectsForRepliesPhase> CreateAsync(ScanProspectsForRepliesPhase phase, CancellationToken ct = default);
        Task<ScanProspectsForRepliesPhase> GetByIdAsync(string scanProspectsForRepliesPhaseId, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}
