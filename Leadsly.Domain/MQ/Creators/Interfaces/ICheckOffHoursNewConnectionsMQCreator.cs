using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators.Interfaces
{
    public interface ICheckOffHoursNewConnectionsMQCreator
    {
        Task PublishMessageAsync(string halId, CancellationToken ct = default);
    }
}
