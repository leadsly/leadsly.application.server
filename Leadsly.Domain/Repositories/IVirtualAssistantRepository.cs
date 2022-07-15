using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IVirtualAssistantRepository
    {
        Task<VirtualAssistant> CreateAsync(VirtualAssistant newVirtualAssistant, CancellationToken ct = default);
        Task<IList<VirtualAssistant>> GetAllByUserIdAsync(string userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(string virtualAssistantId, CancellationToken ct = default);
    }
}
