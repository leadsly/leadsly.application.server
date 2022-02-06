using Leadsly.Domain.Models;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Leadsly.Domain;
using Leadsly.Domain.Providers;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(LeadslyUserManager userManager, 
            IStripeRepository stripeRepository, 
            IContainerRepository containerRepository, 
            ILeadslyBotApiService leadslyBotApiService,
            ICloudPlatformProvider awsContainerProvider,
            ILeadslyProvider leadslyProvider,
            ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _stripeRepository = stripeRepository;
            _leadslyBotApiService = leadslyBotApiService;
            _containerRepository = containerRepository;
            _awsElasticContainerProvider = awsContainerProvider;
            _leadslyProvider = leadslyProvider;
            _logger = logger;
        }

        private readonly LeadslyUserManager _userManager;
        private readonly IContainerRepository _containerRepository;
        private readonly ILeadslyProvider _leadslyProvider;
        private readonly ICloudPlatformProvider _awsElasticContainerProvider;
        private readonly IStripeRepository _stripeRepository;
        private readonly ILogger<Supervisor> _logger;
        private readonly ILeadslyBotApiService _leadslyBotApiService;

    }
}
