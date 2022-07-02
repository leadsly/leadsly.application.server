using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface IUserProvider
    {
        Task<SocialAccount> GetRegisteredSocialAccountAsync(SocialAccountDTO socialAccount, CancellationToken ct = default);
        Task<NewSocialAccountResult> AddUsersSocialAccountAsync(SocialAccountAndResourcesDTO newSocialAccountSetup, CancellationToken ct = default);
        Task<bool> RemoveSocialAccountAndResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default);
        Task<ApplicationUser> GetUserByIdAsync(string userId, CancellationToken ct = default);
        Task<IList<SocialAccount>> GetAllSocialAccounts(CancellationToken ct = default);
        Task<SocialAccount> GetSocialAccountByHalIdAsync(string halId, CancellationToken ct = default);
        Task<SocialAccount> CreateSocialAccountAsync(VirtualAssistant virtualAssistant, string userId, string email, CancellationToken ct = default);
    }
}
