using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IMonitorForNewConnectionsMessagesFactory
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, VirtualAssistant virtualAssistant, MonitorForNewConnectionsPhase phase, CancellationToken ct = default);
    }
}
