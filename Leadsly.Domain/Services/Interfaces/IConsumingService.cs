using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IConsumingService
    {
        Task StartConsumingAsync();
        Task StartConsumingAsync_AllInOneVirtualAssistant();
    }
}
