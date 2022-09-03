using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ICreateFollowUpMessageService
    {
        Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectFollowUpMessagesAsync(string halId, CampaignProspect prospect, IList<FollowUpMessage> followUpMessages, CancellationToken ct = default);
    }
}
