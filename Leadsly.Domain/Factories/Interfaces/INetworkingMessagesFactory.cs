using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface INetworkingMessagesFactory
    {
        Task<IList<NetworkingMessageBody>> CreateNetworkingMessagesAsync(string halId, CancellationToken ct = default);
        Task<IList<NetworkingMessageBody>> CreateNetworkingMessagesAsync(string campaignId, string userId, CancellationToken ct = default);
    }
}
