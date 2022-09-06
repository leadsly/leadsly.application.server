using Leadsly.Application.Model;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class UserProvider : IUserProvider
    {
        public UserProvider(
            ILogger<UserProvider> logger,
            IUserRepository userRepository,
            IScanProspectsForRepliesPhaseRepository scanProspectsForRepliesPhaseRepository,
            ISocialAccountRepository socialAccountRepository,
            IMonitorForNewConnectionsPhaseRepository monitorForNewConnectionsPhaseRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
            _scanProspectsForRepliesPhaseRepository = scanProspectsForRepliesPhaseRepository;
            _monitorForNewConnectionsPhaseRepository = monitorForNewConnectionsPhaseRepository;
            _socialAccountRepository = socialAccountRepository;
        }

        private readonly ILogger<UserProvider> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IScanProspectsForRepliesPhaseRepository _scanProspectsForRepliesPhaseRepository;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly IMonitorForNewConnectionsPhaseRepository _monitorForNewConnectionsPhaseRepository;

        public async Task<ApplicationUser> GetUserByIdAsync(string userId, CancellationToken ct = default)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<IList<SocialAccount>> GetAllSocialAccounts(CancellationToken ct = default)
        {
            return await _userRepository.GetAllSocialAccountsAsync(ct);
        }

        public async Task<SocialAccount> GetSocialAccountByHalIdAsync(string halId, CancellationToken ct = default)
        {
            return await _userRepository.GetSocialAccountByHalIdAsync(halId, ct);
        }

        public async Task<SocialAccount> CreateSocialAccountAsync(VirtualAssistant virtualAssistant, string userId, string email, CancellationToken ct = default)
        {
            SocialAccount newSocialAccount = new()
            {
                Username = email,
                UserId = userId,
                HalDetails = virtualAssistant.HalUnit,
                VirtualAssistant = virtualAssistant,
                Linked = true,
                AccountType = SocialAccountType.LinkedIn
            };

            newSocialAccount = await _socialAccountRepository.AddSocialAccountAsync(newSocialAccount, ct);
            if (newSocialAccount == null)
            {
                _logger.LogError("Failed to create user's social account");
                return null;
            }

            await CreateSocialAccountPhasesAsync(newSocialAccount, ct);

            return newSocialAccount;
        }

        /// <summary>
        /// TODO CONSIDER REMOVING THIS IN THE FUTURE. 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task CreateSocialAccountPhasesAsync(SocialAccount newSocialAccount, CancellationToken ct = default)
        {
            ScanProspectsForRepliesPhase scanForProspectRepliesPhase = new()
            {
                SocialAccount = newSocialAccount,
                PhaseType = PhaseType.ScanForReplies
            };

            await _scanProspectsForRepliesPhaseRepository.CreateAsync(scanForProspectRepliesPhase, ct);

            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = new()
            {
                SocialAccount = newSocialAccount,
                PhaseType = PhaseType.MonitorNewConnections
            };

            await _monitorForNewConnectionsPhaseRepository.CreateAsync(monitorForNewConnectionsPhase, ct);
        }
    }
}
