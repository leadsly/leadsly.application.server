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
    public interface IRabbitMQProvider
    {
        Task<SendConnectionsBody> CreateSendConnectionsBodyAsync(string campaignId, string userId, CancellationToken ct = default);
        Task<MonitorForNewAcceptedConnectionsBody> CreateMonitorForNewAcceptedConnectionsBodyAsync(string halId, string userId, string socialAccountId, CancellationToken ct = default);
        Task<ProspectListBody> CreateProspectListBodyAsync(string prospectListPhaseId, string userId, CancellationToken ct = default);
        Task<ScanProspectsForRepliesBody> CreateScanProspectsForRepliesBodyAsync(string scanProspectsForRepliesPhaseId, string halId, string userId, IList<CampaignProspect> campaignProspects = default, CancellationToken ct = default);
        Task<FollowUpMessageBody> CreateFollowUpMessageBodyAsync(string campaignProspectFollowUpMessageId, string campaignId, CancellationToken ct = default);
        Task<IList<SendConnectionsStageBody>> GetSendConnectionsStagesAsync(string campaignId, int dailyConnectionsLimit, CancellationToken ct = default);
    }
}
