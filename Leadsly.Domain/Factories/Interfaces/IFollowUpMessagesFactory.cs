using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IFollowUpMessagesFactory
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string halId, CampaignProspectFollowUpMessage followUpMessage, FollowUpMessagePhase phase, CancellationToken ct = default);

        Task<FollowUpMessageBody> CreateMessageAsync(string campaignProspectFollowUpMessageId, string campaignId, CancellationToken ct = default);
    }
}
