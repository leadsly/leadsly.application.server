using Leadsly.Domain.Repositories;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Leadsly.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public UserRepository(ILogger<UserRepository> logger, DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private readonly ILogger<UserRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<ApplicationUser> GetByIdAsync(string userId, CancellationToken ct = default)
        {
            ApplicationUser appUser = default;
            try
            {
                appUser = await _dbContext.Users.FindAsync(userId);
                if(appUser == null)
                {
                    _logger.LogError("Failed to get application user by id");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get application user by id");
            }

            return appUser;
        }

        private async Task<bool> UserExistsAsync(string id, CancellationToken ct = default)
        {
            return await GetByIdAsync(id, ct) != null;
        }

        public async Task<IEnumerable<SocialAccount>> GetSocialAccountsByUserIdAsync(string userId, CancellationToken ct = default)
        {
            ApplicationUser applicationUser = await GetByIdAsync(userId);            
            if (applicationUser == null)
            {
                _logger.LogError("Could not find application user. Something went wrong.");
                return null;
            }

            List<SocialAccount> socialAccounts = await _dbContext.SocialAccounts
                .Where(sa => sa.UserId == userId)
                .Include(sa => sa.SocialAccountCloudResource)
                .Include(sa => sa.SocialAccountCloudResource.EcsService)
                    .ThenInclude(ecsSer => ecsSer.EcsServiceRegistries)
                .Include(sa => sa.SocialAccountCloudResource.EcsTaskDefinition)
                .Include(sa => sa.SocialAccountCloudResource.CloudMapServiceDiscoveryService)
                .ToListAsync();

            return socialAccounts;            
        }

        public async Task<IList<SocialAccount>> GetAllSocialAccountsAsync(CancellationToken ct = default)
        {
            IList<SocialAccount> socialAccounts = default;
            try
            {
                socialAccounts = await _dbContext.SocialAccounts.Include(s => s.HalDetails).Include(s => s.User).ThenInclude(u => u.Campaigns).ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all social accounts");                
            }
            return socialAccounts;

        }

        public async Task<SocialAccount> GetSocialAccountByHalIdAsync(string halId, CancellationToken ct = default)
        {
            SocialAccount socialAccount = default;
            try
            {
                socialAccount = await _dbContext.SocialAccounts.Include(s => s.HalDetails).Include(s => s.User).ThenInclude(u => u.Campaigns).Where(socialAccount => socialAccount.HalDetails.HalId == halId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get social account");
            }
            return socialAccount;

        }        
    }
}
