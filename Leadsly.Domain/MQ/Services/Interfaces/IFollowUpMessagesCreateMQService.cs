using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IFollowUpMessagesCreateMQService
    {
        Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectsFollowUpMessagesAsync(string halId, CancellationToken ct = default);

        Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectsFollowUpMessagesAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default);
    }
}
