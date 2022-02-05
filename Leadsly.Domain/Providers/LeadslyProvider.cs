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
    public class LeadslyProvider : ILeadslyProvider
    {
        public LeadslyProvider(ILogger<LeadslyProvider> logger, IUserRepository userRepository, IContainerRepository containerRepository)
        {
            _userRepository = userRepository;
            _containerRepository = containerRepository;
            _logger = logger;
        }

        private readonly ILogger<LeadslyProvider> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IContainerRepository _containerRepository;

        public async Task<SocialAccount> GetSocialAccountAsync(SocialAccountDTO getSocialAccount, CancellationToken ct = default)
        {
            IEnumerable<SocialAccount> socialAccounts = await _userRepository.GetSocialAccountsAsync(getSocialAccount, ct);
            SocialAccount socialAccount = new();

            if(socialAccounts == null)
            {
                socialAccount.DockerContainerCreated = false;
            }

            if(socialAccounts.Count() > 1)
            {
                socialAccount.DuplicateSocialAccountsFound = true;
            }

            if(socialAccounts.Count() == 1)
            {
                
            }
            

            return socialAccount;
        }

        public async Task<DockerContainerInfo> GetContainerInfoBySocialAccountAsync(SocialAccountDTO socialAccountDTO, CancellationToken ct = default)
        {
            ContainerInfoDTO containerInfoDTO = new ContainerInfoDTO();

            ApplicationUser appUser = await _userRepository.GetByIdAsync(socialAccountDTO.UserId);

            SocialAccount socialAccount = appUser.SocialAccounts.FirstOrDefault(s => s.Username == socialAccountDTO.Username && Equals(s.AccountType, socialAccountDTO.AccountType));

            return socialAccount?.DockerContainerInfo;
        }
    }
}
