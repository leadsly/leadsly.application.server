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
            IPrimaryProspectRepository primaryProspectRepository,
            ILeadslyHalApiService leadslyBotApiService,
            ICloudPlatformProvider cloudPlatformProvider,
            IUserProvider userProvider,
            IProspectListPhaseRepository prospectListPhaseRepository,
            ILeadslyHalProvider leadslyHalProvider,
            ICampaignProvider campaignProvider,
            ICampaignManager campaignManager,
            ICampaignService campaignService,
            ISendConnectionsPhaseRepository sendConectionsPhaseRepository,
            ICampaignProspectRepository campaignProspectRepository,
            IMemoryCache memoryCache,            
            ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _campaignProvider = campaignProvider;
            _halRepository = halRepository;
            _sendConnectionsPhaseRepository = sendConectionsPhaseRepository;
            _campaignProspectRepository = campaignProspectRepository;
            _stripeRepository = stripeRepository;            
            _leadslyBotApiService = leadslyBotApiService;
            _cloudPlatformProvider = cloudPlatformProvider;
            _leadslyHalProvider = leadslyHalProvider;
            _prospectListPhaseRepository = prospectListPhaseRepository;
            _campaignManager = campaignManager;
            _socialAccountRepository = socialAccountRepository;
            _memoryCache = memoryCache;
            _userProvider = userProvider;
            _campaignRepository = campaignRepository;
            _rabbitMQRepository = rabbitMQRepository;
            _logger = logger;
            _primaryProspectRepository = primaryProspectRepository;
            _campaignService = campaignService;
        }

        private readonly LeadslyUserManager _userManager;
        private readonly ICampaignProvider _campaignProvider;
        private readonly ICampaignService _campaignService;
        private readonly IHalRepository _halRepository;
        private readonly ISendConnectionsPhaseRepository _sendConnectionsPhaseRepository;
        private readonly ILeadslyHalProvider _leadslyHalProvider;
        private readonly IRabbitMQRepository _rabbitMQRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IUserProvider _userProvider;
        private readonly IProspectListPhaseRepository _prospectListPhaseRepository;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICampaignProspectRepository _campaignProspectRepository;
        private readonly ICloudPlatformProvider _cloudPlatformProvider;
        private readonly IStripeRepository _stripeRepository;
        private readonly ICampaignManager _campaignManager;
        private readonly ILogger<Supervisor> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ILeadslyHalApiService _leadslyBotApiService;
        private readonly IPrimaryProspectRepository _primaryProspectRepository;
    }
}
