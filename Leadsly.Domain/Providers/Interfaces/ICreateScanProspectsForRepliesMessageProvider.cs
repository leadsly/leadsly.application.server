using Leadsly.Application.Model.Campaigns;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICreateScanProspectsForRepliesMessageProvider
    {
        Task<PublishMessageBody> CreateMQScanProspectsForRepliesMessageAsync(string userId, string halId, CancellationToken ct = default);
    }
}
