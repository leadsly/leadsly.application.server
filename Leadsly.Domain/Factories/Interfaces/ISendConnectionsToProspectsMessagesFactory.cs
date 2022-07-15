using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
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
