using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IAllInOneVirtualAssistantFactory
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string halId, CancellationToken ct = default);
    }
}
