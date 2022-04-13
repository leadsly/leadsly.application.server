using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
