using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface IUserProvider
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId, CancellationToken ct = default);
        Task<IList<SocialAccount>> GetAllSocialAccounts(CancellationToken ct = default);
        Task<SocialAccount> GetSocialAccountByHalIdAsync(string halId, CancellationToken ct = default);
        Task<SocialAccount> CreateSocialAccountAsync(VirtualAssistant virtualAssistant, string userId, string email, CancellationToken ct = default);
    }
}
