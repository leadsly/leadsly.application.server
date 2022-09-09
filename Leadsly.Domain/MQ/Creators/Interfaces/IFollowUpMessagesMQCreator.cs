using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Creators.Interfaces
{
    public interface IFollowUpMessagesMQCreator
    {
        Task PublishMessageAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default);
        Task PublishMessageAsync(string halId, CancellationToken ct = default);
    }
}
