using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IPreventFollowUpMessageService
    {
        Task MarkProspectsAsCompleteAsync(string halId);
    }
}
