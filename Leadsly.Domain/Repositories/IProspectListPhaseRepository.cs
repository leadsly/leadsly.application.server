using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IProspectListPhaseRepository
    {
        Task<ProspectListPhase> UpdateAsync(ProspectListPhase prospectListPhase, CancellationToken ct = default);
        Task<IList<ProspectListPhase>> GetAllActiveAsync(CancellationToken ct = default);
        Task<ProspectListPhase> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<ProspectListPhase> GetByIdAsync(string prospectListPhaseId, CancellationToken ct = default);
    }
}
