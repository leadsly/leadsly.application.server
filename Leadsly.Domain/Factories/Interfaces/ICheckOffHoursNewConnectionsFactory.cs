using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface ICheckOffHoursNewConnectionsFactory
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, int numberOfHoursAgo, VirtualAssistant virtualAssistant, MonitorForNewConnectionsPhase phase, CancellationToken ct = default);
    }
}
