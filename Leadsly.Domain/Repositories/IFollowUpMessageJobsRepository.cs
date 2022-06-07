using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IFollowUpMessageJobsRepository
    {
        Task<IList<FollowUpMessageJob>> GetFollowUpJobsAsync(string campaignProspectId, CancellationToken ct = default);
        Task<FollowUpMessageJob> GetFollowUpMessageJobAsync(string followUpMessageId, CancellationToken ct = default);
        Task<bool> DeleteFollowUpMessageJobAsync(string followUpMessageJobId, CancellationToken ct = default);
        Task<FollowUpMessageJob> AddFollowUpJobAsync(FollowUpMessageJob followUpmessageJob, CancellationToken ct = default);
    }
}
