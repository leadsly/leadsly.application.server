using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface INetworkingCreateMQService
    {
        Task<IList<PublishMessageBody>> CreateMQNetworkingMessagesAsync(string halId, CancellationToken ct = default);

        Task<IList<PublishMessageBody>> CreateMQNetworkingMessagesAsync(string halId, Campaign campaign, CancellationToken ct = default);
    }
}
