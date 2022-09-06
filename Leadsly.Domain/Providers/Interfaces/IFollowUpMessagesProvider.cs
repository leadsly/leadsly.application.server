using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface IFollowUpMessagesProvider
    {
        Task PublishMessageAsync(string halId, CancellationToken ct = default);
    }
}
