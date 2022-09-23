using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IAllInOneVirtualAssistantMQService
    {
        Task<PublishMessageBody> CreateMQAllInOneVirtualAssistantMessageAsync(string halId, bool initial, CancellationToken ct = default);
        Task<bool> ProvisionResourcesAsync(string halId, string userId, CancellationToken ct = default);
    }
}
