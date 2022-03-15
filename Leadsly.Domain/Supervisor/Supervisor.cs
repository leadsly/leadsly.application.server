using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Domain.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(LeadslyUserManager userManager, 
            IStripeRepository stripeRepository, 
            ILeadslyHalApiService leadslyBotApiService,
            ICloudPlatformProvider cloudPlatformProvider,
            ILeadslyHalProvider leadslyHalProvider,
            ISocialAccountRepository socialAccountRepository,
            IMemoryCache memoryCache,
            ICampaignRepository campaignRepository,
            IRabbitMQRepository rabbitMQRepository,
            IUserProvider userProvider,            
            ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _stripeRepository = stripeRepository;
            _leadslyBotApiService = leadslyBotApiService;
            _cloudPlatformProvider = cloudPlatformProvider;
            _leadslyHalProvider = leadslyHalProvider;
            _socialAccountRepository = socialAccountRepository;
            _memoryCache = memoryCache;
            _userProvider = userProvider;
            _campaignRepository = campaignRepository;
            _rabbitMQRepository = rabbitMQRepository;
            _logger = logger;            
        }

        private readonly LeadslyUserManager _userManager;
        
        private readonly ILeadslyHalProvider _leadslyHalProvider;
        private readonly IRabbitMQRepository _rabbitMQRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IUserProvider _userProvider;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICloudPlatformProvider _cloudPlatformProvider;
        private readonly IStripeRepository _stripeRepository;        
        private readonly ILogger<Supervisor> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ILeadslyHalApiService _leadslyBotApiService;

    }
}
