using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns.MonitorForNewProspects;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface IMonitorNewConnectionsProvider
    {
        public Task<HalOperationResult<T>> ProcessNewConnectionsAsync<T>(List<NewConnectionProspect> newConnectionProspects, string halId, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
