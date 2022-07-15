using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IFollowUpMessageRepository
    {
        Task<IList<FollowUpMessage>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default);
        Task<CampaignProspectFollowUpMessage> CreateAsync(CampaignProspectFollowUpMessage message, CancellationToken ct = default);
        Task<CampaignProspectFollowUpMessage> GetCampaignProspectFollowUpMessageByIdAsync(string campaignProspectFollowUpMessageId, CancellationToken ct = default);
    }
}
