using Leadsly.Application.Model.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IMonitorForNewConnectionsMessagesFactory
    {
        Task<IList<MonitorForNewAcceptedConnectionsBody>> CreateMessagesAsync(int numOfHoursAgo = 0, CancellationToken ct = default);
        /// <summary>
        /// Creates a message body with instructions to only check for new prospects from the last 12 hours.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="numOfHoursAgo"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MonitorForNewAcceptedConnectionsBody> CreateMessageAsync(string halId, int numOfHoursAgo, CancellationToken ct = default);
        Task<MonitorForNewAcceptedConnectionsBody> CreateMessageAsync(string halId, CancellationToken ct = default);
    }
}
