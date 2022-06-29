using Leadsly.Application.Model.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IVirtualAssistantRepository
    {
        Task<VirtualAssistant> CreateAsync(VirtualAssistant newVirtualAssistant, CancellationToken ct = default);
    }
}
