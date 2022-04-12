using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades.Interfaces
{
    public interface ICampaignRepositoryFacade
    {
        #region Campaign
        Task<Campaign> CreateCampaignAsync(Campaign newCampaign, CancellationToken ct = default);
        Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign, CancellationToken ct = default);
        Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveCampaignsByUserIdAsync(string applicationUserId, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveCampaignsAsync(CancellationToken ct = default);
        Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default);
        Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default);
        #endregion

        #region CampaignProspect
        Task<IList<CampaignProspect>> CreateAllCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllCampaignProspectsByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> UpdateAllCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<CampaignProspectList> GetCampaignProspectListByListIdAsync(string campaignProspectListId, CancellationToken ct = default);
        #endregion
    }
}
