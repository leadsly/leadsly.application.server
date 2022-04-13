using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IFollowUpMessagePhaseRepository
    {
        Task<FollowUpMessagePhase> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default);
    }
}
