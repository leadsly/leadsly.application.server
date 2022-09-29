using Leadsly.Domain.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IDeprovisionResourcesRepository
    {
        Task<VirtualAssistant> UpdateVirtualAssistantAsync(VirtualAssistant updated, CancellationToken ct = default);
        Task<VirtualAssistant> GetVirtualAssistantByHalIdAsync(string halId, CancellationToken ct = default);
    }
}
