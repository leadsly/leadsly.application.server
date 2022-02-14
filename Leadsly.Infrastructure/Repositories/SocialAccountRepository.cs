using Leadsly.Domain.Repositories;
using Leadsly.Models.Entities;
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

        public async Task<SocialAccount> AddSocialAccountAsync(SocialAccount newSocialAccount, CancellationToken ct = default)
        {
            try
            {
                _dbContext.SocialAccounts.Add(newSocialAccount);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to add new social account.");
            }
            return newSocialAccount;
        }
    }
}
