using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ICreateScanProspectsForRepliesMessageService
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, ScanProspectsForRepliesPhase phase, CancellationToken ct = default);
    }
}
