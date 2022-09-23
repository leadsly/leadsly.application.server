using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IAllInOneVirtualAssistantCreateMQService
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string halId, CancellationToken ct = default);
        Task SetCheckOffHoursNewConnectionsProperties(string halId, bool initial, AllInOneVirtualAssistantMessageBody mqMessage, CancellationToken ct = default);
        Task SetDeepScanProspectsForRepliesProperties(string halId, bool initial, AllInOneVirtualAssistantMessageBody mqMessage, CancellationToken ct = default);
    }
}
