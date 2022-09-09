using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IFollowUpMessagesFactory
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string halId, CampaignProspectFollowUpMessage followUpMessage, FollowUpMessagePhase phase, CancellationToken ct = default);
    }
}
