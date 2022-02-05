using Leadsly.Domain.Repositories;
using Leadsly.Models;
using Leadsly.Models.Entities;
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

        public async Task<ApplicationUser> GetByIdAsync(string walletId, CancellationToken ct = default)
        {
            ApplicationUser wallet = await _dbContext.Users.FindAsync(walletId);

            if (wallet == null)
            {
                // log here
            }

            return wallet;
        }

        private async Task<bool> UserExistsAsync(string id, CancellationToken ct = default)
        {
            return await GetByIdAsync(id, ct) != null;
        }

        public async Task<IEnumerable<SocialAccount>> GetSocialAccountsAsync(SocialAccountDTO getSocialAccount, CancellationToken ct = default)
        {
            ApplicationUser applicationUser = await GetByIdAsync(getSocialAccount.UserId);

            if(applicationUser == null)
            {
                throw new Exception("User does not exist");
            }

            IEnumerable<SocialAccount> socialAccounts = applicationUser.SocialAccounts.Where(s => s.Username == getSocialAccount.Username && Equals(s.AccountType, getSocialAccount.AccountType));
            return socialAccounts;
        }
    }
}
