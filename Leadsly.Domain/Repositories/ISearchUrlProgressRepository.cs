using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ISearchUrlProgressRepository
    {
        Task<SearchUrlProgress> CreateAsync(SearchUrlProgress searchUrlProgress, CancellationToken ct = default);

        Task<SearchUrlProgress> UpdateAsync(SearchUrlProgress searchUrlProgress, CancellationToken ct = default);

        Task<IList<SearchUrlProgress>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default);
    }
}
