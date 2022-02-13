using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Microsoft.Extensions.Logging;
using Leadsly.Domain.Providers;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(LeadslyUserManager userManager, 
            IStripeRepository stripeRepository, 
            IContainerRepository containerRepository,
            ICloudPlatformRepository cloudPlatformRepository,
            ILeadslyBotApiService leadslyBotApiService,
            ICloudPlatformProvider cloudPlatformProvider,
            ILeadslyProvider leadslyProvider,
            ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _stripeRepository = stripeRepository;
            _leadslyBotApiService = leadslyBotApiService;
            _containerRepository = containerRepository;
            _cloudPlatformProvider = cloudPlatformProvider;
            _leadslyProvider = leadslyProvider;            
            _logger = logger;
        }

        private readonly LeadslyUserManager _userManager;
        private readonly IContainerRepository _containerRepository;
        private readonly ILeadslyProvider _leadslyProvider;
        private readonly ICloudPlatformProvider _cloudPlatformProvider;
        private readonly IStripeRepository _stripeRepository;        
        private readonly ILogger<Supervisor> _logger;
        private readonly ILeadslyBotApiService _leadslyBotApiService;

    }
}
