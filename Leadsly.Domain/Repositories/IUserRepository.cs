using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<SocialAccount>> GetSocialAccountsByUserIdAsync(string userId, CancellationToken ct = default);
        Task<ApplicationUser> GetByIdAsync(string walletId, CancellationToken ct = default);
        Task<IList<SocialAccount>> GetAllSocialAccountsAsync(CancellationToken ct = default);
        Task<SocialAccount> GetSocialAccountByHalIdAsync(string halId, CancellationToken ct = default);
    }
}
