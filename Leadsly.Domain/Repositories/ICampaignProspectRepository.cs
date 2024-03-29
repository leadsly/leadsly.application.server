﻿using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ICampaignProspectRepository
    {
        Task<IList<CampaignProspect>> CreateAllAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> GetAllFollowUpMessageEligbleByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<IList<CampaignProspect>> UpdateAllAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task<CampaignProspect> UpdateAsync(CampaignProspect updatedCampaignProspect, CancellationToken ct = default);
        Task<CampaignProspect> GetByIdAsync(string campaignProspectId, CancellationToken ct = default);
        Task<CampaignProspect> GetByProfileUrlAsync(string profileUrl, string halId, string userId, CancellationToken ct = default);
        Task<CampaignProspectList> GetListByListIdAsync(string campaignProspectListId, CancellationToken ct = default);
    }
}
