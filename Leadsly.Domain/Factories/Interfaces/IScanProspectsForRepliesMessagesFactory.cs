using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IScanProspectsForRepliesMessagesFactory
    {
        Task<ScanProspectsForRepliesBody> CreateMessageAsync(string halId, CancellationToken ct = default);

        Task<DeepScanProspectsForRepliesBody> CreateDeepScanMessageAsync(string halId, CancellationToken ct = default);

        Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, VirtualAssistant virtualAssistant, ScanProspectsForRepliesPhase phase, CancellationToken ct = default);
    }
}
