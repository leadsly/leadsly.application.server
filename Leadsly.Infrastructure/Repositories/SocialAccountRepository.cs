using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class SocialAccountRepository : ISocialAccountRepository
    {
        public SocialAccountRepository(DatabaseContext dbContext, ILogger<SocialAccountRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<SocialAccountRepository> _logger;

        private async Task<bool> SocialAccountExistsAsync(string id, CancellationToken ct = default)
        {
            return await _dbContext.SocialAccounts.AnyAsync(s => s.SocialAccountId == id, ct);
        }

        public async Task<SocialAccount> AddSocialAccountAsync(SocialAccount newSocialAccount, CancellationToken ct = default)
        {
            try
            {
                _dbContext.SocialAccounts.Add(newSocialAccount);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add new social account.");
                return null;
            }
            return newSocialAccount;
        }

        public async Task<bool> RemoveSocialAccountAsync(string id, CancellationToken ct = default)
        {
            if (!await SocialAccountExistsAsync(id, ct))
            {
                _logger.LogWarning("Social account does not exist. SocialAccountId: {id}", id);
                return false;
            }
            bool result = false;
            try
            {
                SocialAccount toRemove = _dbContext.SocialAccounts.Find(id);
                _dbContext.SocialAccounts.Remove(toRemove);
                await _dbContext.SaveChangesAsync(ct);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Something went wrong removing user's social account");
                result = false;
            }
            return result;
        }

        public async Task<SocialAccount> GetByIdAsync(string id, CancellationToken ct = default)
        {
            SocialAccount socialAccount = default;
            try
            {
                socialAccount = await _dbContext.SocialAccounts
                    .Include(s => s.ConnectionWithdrawPhase)
                    .Include(s => s.ScanProspectsForRepliesPhase)
                    .Include(s => s.MonitorForNewProspectsPhase)
                    .FirstOrDefaultAsync(s => s.SocialAccountId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve SocialAccount by id");
                return null;
            }
            return socialAccount;
        }

        public async Task<SocialAccount> UpdateAsync(SocialAccount updatedSocialAccount, CancellationToken ct = default)
        {
            string socialAccountId = updatedSocialAccount?.SocialAccountId;
            try
            {
                _dbContext.SocialAccounts.Update(updatedSocialAccount);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully udpated SocialAccount with id {socialAccountId}", socialAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update social account. Social account id {socialAccountId}", socialAccountId);
                return null;
            }

            return updatedSocialAccount;
        }

        public async Task<SocialAccount> GetByUserNameAsync(string email, CancellationToken ct = default)
        {
            SocialAccount socialAccount = default;
            try
            {
                socialAccount = await _dbContext.SocialAccounts.Where(s => s.Username == email).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve SocialAccount by email {email}", email);
                return null;
            }
            return socialAccount;
        }

        public async Task<IList<SocialAccount>> GetAllAsync(CancellationToken ct = default)
        {
            IList<SocialAccount> allSocialAccounts = default;
            try
            {
                allSocialAccounts = await _dbContext.SocialAccounts.Include(s => s.HalDetails).ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all SocialAccounts");
                return null;
            }
            return allSocialAccounts;
        }

        public async Task<SocialAccount> GetByUserIdAsync(string userId, CancellationToken ct = default)
        {
            SocialAccount socialAccount = default;
            try
            {
                socialAccount = await _dbContext.SocialAccounts.Where(s => s.UserId == userId).Include(s => s.HalDetails).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve SocialAccount by user id {userId}", userId);
                return null;
            }
            return socialAccount;
        }
    }
}
