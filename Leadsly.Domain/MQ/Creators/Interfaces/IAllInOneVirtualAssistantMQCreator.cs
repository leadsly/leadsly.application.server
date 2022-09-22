using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators.Interfaces
{
    public interface IAllInOneVirtualAssistantMQCreator
    {
        Task PublishMessageAsync(string halId, bool initial, CancellationToken ct = default);
    }
}
