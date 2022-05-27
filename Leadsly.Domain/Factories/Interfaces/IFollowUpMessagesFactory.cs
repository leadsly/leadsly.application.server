using Leadsly.Application.Model.Campaigns;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IFollowUpMessagesFactory
    {
        Task<FollowUpMessageBody> CreateMessageAsync(string campaignProspectFollowUpMessageId, string campaignId, CancellationToken ct = default);
    }
}
