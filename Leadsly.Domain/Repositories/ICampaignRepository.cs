using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ICampaignRepository
    {
        Task<Campaign> CreateAsync(Campaign newCampaign, CancellationToken ct = default);
        Task<Campaign> UpdateAsync(Campaign updatedCampaign, CancellationToken ct = default);        
        Task<Campaign> GetByIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveByUserIdAsync(string applicationUserId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveAsync(CancellationToken ct = default);
        Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default);
        Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default);
    }
}
