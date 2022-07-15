using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IFollowUpMessagePhaseRepository
    {
        Task<FollowUpMessagePhase> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default);
    }
}
