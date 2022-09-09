using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators.Interfaces
{
    public interface IMonitorForNewConnectionsMQCreator
    {
        Task PublishMessageAsync(string halId, CancellationToken ct = default);
    }
}
