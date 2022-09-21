using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IRecentlyAddedProspectRepository
    {
        Task<IList<RecentlyAddedProspect>> GetAllBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default);
        Task<IList<RecentlyAddedProspect>> CreateAllAsync(IList<RecentlyAddedProspect> newEntites, CancellationToken ct = default);
        Task<bool> DeleteAllBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default);
    }
}
