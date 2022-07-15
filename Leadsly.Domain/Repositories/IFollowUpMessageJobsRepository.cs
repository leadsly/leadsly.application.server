using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IFollowUpMessageJobsRepository
    {
        Task<IList<FollowUpMessageJob>> GetAllByCampaignProspectIdAsync(string campaignProspectId, CancellationToken ct = default);
        Task<FollowUpMessageJob> GetByFollowUpmessageIdAsync(string followUpMessageId, CancellationToken ct = default);
        Task<bool> DeleteFollowUpMessageJobAsync(string followUpMessageJobId, CancellationToken ct = default);
        Task<FollowUpMessageJob> AddFollowUpJobAsync(FollowUpMessageJob followUpmessageJob, CancellationToken ct = default);
    }
}
