using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface ISendConnectionsToProspectsMessagesFactory
    {
        Task<SendConnectionsBody> CreateMessageAsync(Campaign activeCampaign, CampaignWarmUp campaignWarmUp, CancellationToken ct = default);
        IList<SendConnectionsStageBody> CreateStages(IList<SendConnectionsStage> sendConnectionsStages, int dailyConnectionsLimit, CancellationToken ct = default);
    }
}
