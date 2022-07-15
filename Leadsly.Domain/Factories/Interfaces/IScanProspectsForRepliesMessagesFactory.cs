using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IScanProspectsForRepliesMessagesFactory
    {
        Task<ScanProspectsForRepliesBody> CreateMessageAsync(string halId, IList<CampaignProspect> campaignProspects = default, CancellationToken ct = default);
    }
}
