﻿using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

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
            ICreateCampaignService createCampaignService,
            ITimestampService timestampService,
            ISearchUrlProgressRepository searchUrlProgressRepository,
            ICampaignProvider campaignProvider,
            ISocialAccountRepository socialAccountRepository,
            ICampaignPhaseClient campaignPhaseClient,
            ITimeZoneRepository timeZoneRepository,
            ICampaignPhaseProcessorProvider campaignPhaseProcessorProvider,
            IVirtualAssistantRepository virtualAssistantRepository,
            IMemoryCache memoryCache,
            ILogger<Supervisor> logger)
        {
            _timestampService = timestampService;
            _timeZoneRepository = timeZoneRepository;
            _campaignProvider = campaignProvider;
            _virtualAssistantRepository = virtualAssistantRepository;
            _socialAccountRepository = socialAccountRepository;
            _halRepository = halRepository;
            _campaignPhaseClient = campaignPhaseClient;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _stripeRepository = stripeRepository;
            _cloudPlatformProvider = cloudPlatformProvider;
            _leadslyHalProvider = leadslyHalProvider;
            _searchUrlProgressRepository = searchUrlProgressRepository;
            _memoryCache = memoryCache;
            _campaignPhaseProcessorProvider = campaignPhaseProcessorProvider;
            _userProvider = userProvider;
            _createCampaignService = createCampaignService;
            _logger = logger;
        }

        private readonly ITimestampService _timestampService;
        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly ICreateCampaignService _createCampaignService;
        private readonly ICampaignPhaseProcessorProvider _campaignPhaseProcessorProvider;
        private readonly ITimeZoneRepository _timeZoneRepository;
        private readonly ICampaignProvider _campaignProvider;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ICampaignPhaseClient _campaignPhaseClient;
        private readonly IHalRepository _halRepository;
        private readonly ILeadslyHalProvider _leadslyHalProvider;
        private readonly ISearchUrlProgressRepository _searchUrlProgressRepository;
        private readonly IUserProvider _userProvider;
        private readonly ICloudPlatformProvider _cloudPlatformProvider;
        private readonly IStripeRepository _stripeRepository;
        private readonly ILogger<Supervisor> _logger;
        private readonly IMemoryCache _memoryCache;
    }
}
