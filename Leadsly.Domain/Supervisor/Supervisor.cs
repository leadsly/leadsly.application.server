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
        public Supervisor(
            IStripeRepository stripeRepository,
            ICampaignRepositoryFacade campaignRepositoryFacade,            
            IHalRepository halRepository,     
            ICloudPlatformProvider cloudPlatformProvider,
            IUserProvider userProvider,            
            ILeadslyHalProvider leadslyHalProvider,
            ICampaignProvider campaignProvider,
            ICampaignPhaseClient campaignPhaseClient,
            ICampaignPhaseProcessorProvider campaignPhaseProcessorProvider,
            IMemoryCache memoryCache,            
            ILogger<Supervisor> logger)
        {
            _campaignProvider = campaignProvider;
            _halRepository = halRepository;
            _campaignPhaseClient = campaignPhaseClient;
            _campaignRepositoryFacade = campaignRepositoryFacade; 
            _stripeRepository = stripeRepository;          
            _cloudPlatformProvider = cloudPlatformProvider;
            _leadslyHalProvider = leadslyHalProvider;                        
            _memoryCache = memoryCache;
            _campaignPhaseProcessorProvider = campaignPhaseProcessorProvider;
            _userProvider = userProvider;            
            _logger = logger;            
        }

        private readonly ICampaignPhaseProcessorProvider _campaignPhaseProcessorProvider;
        private readonly ICampaignProvider _campaignProvider;        
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ICampaignPhaseClient _campaignPhaseClient;
        private readonly IHalRepository _halRepository;        
        private readonly ILeadslyHalProvider _leadslyHalProvider;    
        private readonly IUserProvider _userProvider;               
        private readonly ICloudPlatformProvider _cloudPlatformProvider;
        private readonly IStripeRepository _stripeRepository;
        private readonly ILogger<Supervisor> _logger;
        private readonly IMemoryCache _memoryCache;     
    }
}
