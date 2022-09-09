using Leadsly.Domain.Models.Entities.Campaigns;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators.Interfaces
{
    public interface INetworkingMQCreator
    {
        Task PublishMessageAsync(string halId, CancellationToken ct = default);
        Task PublishMessageAsync(string halId, Campaign campaign, CancellationToken ct = default);
    }
}
