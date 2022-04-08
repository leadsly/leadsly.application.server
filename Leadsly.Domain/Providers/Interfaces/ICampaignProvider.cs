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

        Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken ct = default);

        Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(string campaignId, string userId, CancellationToken ct = default);

        Task<MonitorForNewAcceptedConnectionsBody> CreateMonitorForNewAcceptedConnectionsBodyAsync(string halId, string userId, CancellationToken ct = default);

        Task<IList<SendConnectionsStageBody>> GetSendConnectionsStagesAsync(string campaignId, int dailyConnectionsLimit, CancellationToken ct = default);

        Task<ProspectListBody> CreateProspectListBodyAsync(string propsectListId, string userId, CancellationToken ct = default);

        CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId);
    }
}
