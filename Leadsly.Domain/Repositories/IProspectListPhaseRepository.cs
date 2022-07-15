using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IProspectListPhaseRepository
    {
        Task<ProspectListPhase> UpdateAsync(ProspectListPhase prospectListPhase, CancellationToken ct = default);
        Task<IList<ProspectListPhase>> GetAllActiveAsync(CancellationToken ct = default);
        Task<IList<ProspectListPhase>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default);
        Task<ProspectListPhase> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<ProspectListPhase> GetByIdAsync(string prospectListPhaseId, CancellationToken ct = default);
        Task<bool> AnyIncompleteByHalIdAsync(string halId, CancellationToken ct = default);
    }
}
