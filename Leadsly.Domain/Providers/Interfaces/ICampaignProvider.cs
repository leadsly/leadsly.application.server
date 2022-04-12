using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICampaignProvider
    {
        void ProcessNewCampaign(Campaign newCampaign);

        Task<HalsProspectListPhasesPayload> GetActiveProspectListPhasesAsync(CancellationToken ct = default);
        Task<List<string>> HalIdsWithActiveCampaignsAsync(CancellationToken ct = default);
        CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId);
    }
}
