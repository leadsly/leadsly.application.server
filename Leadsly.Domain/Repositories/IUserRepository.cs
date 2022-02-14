using Leadsly.Models;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<SocialAccount>> GetSocialAccountsByUserIdAsync(string userId, CancellationToken ct = default);
        Task<ApplicationUser> GetByIdAsync(string walletId, CancellationToken ct = default);
    }
}
