using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IMonitorForNewConnectionsMessagesFactory
    {
        Task<IList<MonitorForNewAcceptedConnectionsBody>> CreateMessagesAsync(int numOfHoursAgo = 0, CancellationToken ct = default);
        Task<MonitorForNewAcceptedConnectionsBody> CreateMessageAsync(string halId, int numOfHoursAgo = 0, CancellationToken ct = default);
        Task<MonitorForNewAcceptedConnectionsBody> CreateMessageAsync(string halId, CancellationToken ct = default);
    }
}
