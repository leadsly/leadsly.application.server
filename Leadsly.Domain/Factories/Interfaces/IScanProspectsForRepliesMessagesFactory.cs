using Leadsly.Application.Model.Campaigns;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IScanProspectsForRepliesMessagesFactory
    {
        Task<ScanProspectsForRepliesBody> CreateMessageAsync(string halId, CancellationToken ct = default);

        Task<DeepScanProspectsForRepliesBody> CreateDeepScanMessageAsync(string halId, CancellationToken ct = default);
    }
}
