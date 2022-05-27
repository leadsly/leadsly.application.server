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
    public interface IScanProspectsForRepliesMessagesFactory
    {
        Task<ScanProspectsForRepliesBody> CreateMessageAsync(string halId, IList<CampaignProspect> campaignProspects = default, CancellationToken ct = default);
    }
}
