using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Domain.Providers;
using Microsoft.Extensions.Logging;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(LeadslyUserManager userManager, 
            IStripeRepository stripeRepository, 
            ICloudPlatformRepository cloudPlatformRepository,
            ILeadslyBotApiService leadslyBotApiService,
            ICloudPlatformProvider cloudPlatformProvider,
            ISocialAccountRepository socialAccountRepository,
            IUserProvider userProvider,
            ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _stripeRepository = stripeRepository;
            _leadslyBotApiService = leadslyBotApiService;
            _cloudPlatformProvider = cloudPlatformProvider;
            _socialAccountRepository = socialAccountRepository;
            _userProvider = userProvider;            
            _logger = logger;            
        }

        private readonly LeadslyUserManager _userManager;
        private readonly IUserProvider _userProvider;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICloudPlatformProvider _cloudPlatformProvider;
        private readonly IStripeRepository _stripeRepository;        
        private readonly ILogger<Supervisor> _logger;
        private readonly ILeadslyBotApiService _leadslyBotApiService;

    }
}
