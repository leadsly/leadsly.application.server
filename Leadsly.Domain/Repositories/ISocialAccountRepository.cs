﻿using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ISocialAccountRepository
    {
        Task<SocialAccount> AddSocialAccountAsync(SocialAccount newSocialAccount, CancellationToken ct = default);
        Task<SocialAccount> GetByIdAsync(string id, CancellationToken ct = default);
        Task<SocialAccount> GetByUserIdAsync(string userId, CancellationToken ct = default);
        Task<SocialAccount> GetByUserNameAsync(string email, CancellationToken ct = default);
        Task<SocialAccount> UpdateAsync(SocialAccount updatedSocialAccount, CancellationToken ct = default);
        Task<bool> RemoveSocialAccountAsync(string id, CancellationToken ct = default);
        Task<IList<SocialAccount>> GetAllAsync(CancellationToken ct = default);
    }
}
