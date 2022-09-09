using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IDeepScanProspectsForRepliesCreateMQService
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, string phaseId, CancellationToken ct = default);
    }
}
