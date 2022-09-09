using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface INetworkingMessagesFactory
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string halId, string startTime, int numberOfProspectsToCrawl, Campaign campaign, PrimaryProspectList primaryProspectList, CancellationToken ct = default);
    }
}
