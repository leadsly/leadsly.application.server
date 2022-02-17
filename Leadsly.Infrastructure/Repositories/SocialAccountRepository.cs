using Leadsly.Domain.Repositories;
using Leadsly.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return await _dbContext.SocialAccounts.AnyAsync(s => s.Id == id, ct);
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
                _logger.LogError(ex,"Failed to add new social account.");
                // detach any tracked items so that any future insertions can proceed successfully
                _dbContext.Entry(newSocialAccount).State = EntityState.Detached;
                _dbContext.Entry(newSocialAccount.SocialAccountCloudResource).State = EntityState.Detached;
                _dbContext.Entry(newSocialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService).State = EntityState.Detached;
                _dbContext.Entry(newSocialAccount.SocialAccountCloudResource.EcsService).State = EntityState.Detached;
                _dbContext.Entry(newSocialAccount.SocialAccountCloudResource.EcsTaskDefinition).State = EntityState.Detached;
                return null;
            }
            return newSocialAccount;
        }

        public async Task<bool> RemoveSocialAccountAsync(string id, CancellationToken ct = default)
        {
            if(!await SocialAccountExistsAsync(id, ct))
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
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Something went wrong removing user's social account");
                result = false;
            }
            return result;
        }
    }
}
