using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ISearchUrlProgressRepository
    {
        Task<SearchUrlProgress> CreateAsync(SearchUrlProgress searchUrlProgress, CancellationToken ct = default);

        Task<SearchUrlProgress> UpdateAsync(SearchUrlProgress searchUrlProgress, CancellationToken ct = default);

        Task<SearchUrlProgress> GetByIdAsync(string searchUrlProgressId, CancellationToken ct = default);

        Task<IList<SearchUrlProgress>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default);
    }
}
