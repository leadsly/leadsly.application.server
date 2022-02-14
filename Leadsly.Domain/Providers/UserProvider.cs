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

namespace Leadsly.Domain.Providers
{
    public class UserProvider : IUserProvider
    {
        public UserProvider(ILogger<UserProvider> logger, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        private readonly ILogger<UserProvider> _logger;
        private readonly IUserRepository _userRepository;

        public async Task<SocialAccount> GetRegisteredSocialAccountAsync(SocialAccountDTO socialAccountDTO, CancellationToken ct = default)
        {
            IEnumerable<SocialAccount> socialAccounts = await _userRepository.GetSocialAccountsByUserIdAsync(socialAccountDTO.UserId, ct);
            if(socialAccounts == null)
            {
                return null;
            }

            SocialAccount socialAccount = socialAccounts.FirstOrDefault(s => s.Username == socialAccountDTO.Username);
            return socialAccount;
        }

    }
}
