using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ICampaignProspectRepository
    {
        Task<IList<CampaignProspect>> CreateAllAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> UpdateAllAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<CampaignProspectList> GetListByListIdAsync(string campaignProspectListId, CancellationToken ct = default);        
    }
}
