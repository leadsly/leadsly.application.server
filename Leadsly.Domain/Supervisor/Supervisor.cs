﻿using Leadsly.Domain.Decorators;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(
            IStripeRepository stripeRepository,
            IProvisionResourcesService provisionResourcesService,
            SaveBrowserProfileUserProviderDecorator saveBrowserProfileUserProvider,
            IMQCreatorFacade mqCreatorFacade,
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
            ICloudPlatformRepository cloudPlatformRepository,
            IFollowUpMessageJobsRepository followUpMessageJobRepository,
            IHangfireService hangfireService,
            ITimeZoneRepository timeZoneRepository,
            IVirtualAssistantRepository virtualAssistantRepository,
            IDeprovisionResourcesProvider deprovisionResourcesProvider,
            IRecentlyAddedProspectRepository recentlyAddedRepository,
            IFollowUpMessagesMQService followUpMessagesMQService,
            IMemoryCache memoryCache,
            IOptions<FeatureFlagsOptions> options,
            ILogger<Supervisor> logger)
        {
            _featureFlagsOptions = options.Value;
            _deprovisionResourcesProvider = deprovisionResourcesProvider;
            _followUpMessagesMQService = followUpMessagesMQService;
            _provisionResourcesService = provisionResourcesService;
            _recentlyAddedRepository = recentlyAddedRepository;
            _saveBrowserProfileUserProvider = saveBrowserProfileUserProvider;
            _mqCreatorFacade = mqCreatorFacade;
            _hangfireService = hangfireService;
            _followUpMessageJobRepository = followUpMessageJobRepository;
            _cloudPlatformRepository = cloudPlatformRepository;
            _timestampService = timestampService;
            _timeZoneRepository = timeZoneRepository;
            _campaignProvider = campaignProvider;
            _virtualAssistantRepository = virtualAssistantRepository;
            _socialAccountRepository = socialAccountRepository;
            _halRepository = halRepository;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _stripeRepository = stripeRepository;
            _cloudPlatformProvider = cloudPlatformProvider;
            _leadslyHalProvider = leadslyHalProvider;
            _searchUrlProgressRepository = searchUrlProgressRepository;
            _memoryCache = memoryCache;
            _userProvider = userProvider;
            _createCampaignService = createCampaignService;
            _logger = logger;
        }

        private readonly FeatureFlagsOptions _featureFlagsOptions;
        private readonly IDeprovisionResourcesProvider _deprovisionResourcesProvider;
        private readonly IFollowUpMessagesMQService _followUpMessagesMQService;
        private readonly IProvisionResourcesService _provisionResourcesService;
        private readonly IRecentlyAddedProspectRepository _recentlyAddedRepository;
        private readonly SaveBrowserProfileUserProviderDecorator _saveBrowserProfileUserProvider;
        private readonly IMQCreatorFacade _mqCreatorFacade;
        private readonly IHangfireService _hangfireService;
        private readonly IFollowUpMessageJobsRepository _followUpMessageJobRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ITimestampService _timestampService;
        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly ICreateCampaignService _createCampaignService;
        private readonly ITimeZoneRepository _timeZoneRepository;
        private readonly ICampaignProvider _campaignProvider;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
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
