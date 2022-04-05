using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Domain.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Leadsly.Domain.Services.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Facades.Interfaces;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(LeadslyUserManager userManager, 
            IStripeRepository stripeRepository,
            ISocialAccountRepository socialAccountRepository,
            ICampaignRepository campaignRepository,
            IHalRepository halRepository,
            IRabbitMQRepository rabbitMQRepository,
            IProspectListRepository prospectListRepository,            
            ILeadslyHalApiService leadslyBotApiService,
            ICloudPlatformProvider cloudPlatformProvider,
            IUserProvider userProvider,
            ILeadslyHalProvider leadslyHalProvider,
            ICampaignProvider campaignProvider,
            ICampaignManager campaignManager,
            ICampaignService campaignService,
            IMemoryCache memoryCache,            
            ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _campaignProvider = campaignProvider;
            _halRepository = halRepository;
            _stripeRepository = stripeRepository;            
            _leadslyBotApiService = leadslyBotApiService;
            _prospectListRepository = prospectListRepository;
            _cloudPlatformProvider = cloudPlatformProvider;
            _leadslyHalProvider = leadslyHalProvider;
            _campaignManager = campaignManager;
            _socialAccountRepository = socialAccountRepository;
            _memoryCache = memoryCache;
            _userProvider = userProvider;
            _campaignRepository = campaignRepository;
            _rabbitMQRepository = rabbitMQRepository;
            _logger = logger;
            _campaignService = campaignService;
        }

        private readonly LeadslyUserManager _userManager;
        private readonly ICampaignProvider _campaignProvider;
        private readonly ICampaignService _campaignService;
        private readonly IHalRepository _halRepository;
        private readonly ILeadslyHalProvider _leadslyHalProvider;
        private readonly IRabbitMQRepository _rabbitMQRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IUserProvider _userProvider;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICloudPlatformProvider _cloudPlatformProvider;
        private readonly IStripeRepository _stripeRepository;
        private readonly ICampaignManager _campaignManager;
        private readonly ILogger<Supervisor> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ILeadslyHalApiService _leadslyBotApiService;
        private readonly IProspectListRepository _prospectListRepository;
    }
}
