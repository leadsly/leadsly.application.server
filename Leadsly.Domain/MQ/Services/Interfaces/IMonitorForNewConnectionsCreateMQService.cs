using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IMonitorForNewConnectionsCreateMQService
    {
        Task<PublishMessageBody> CreateMQMonitorForNewConnectionsMessageAsync(string userId, string halId, MonitorForNewConnectionsPhase phase, CancellationToken ct = default);
    }
}
