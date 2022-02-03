using Leadsly.Domain.Models;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Leadsly.Domain;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(LeadslyUserManager userManager, IStripeRepository stripeRepository, ILeadslyBotApiService leadslyBotApiService, ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _stripeRepository = stripeRepository;
            _leadslyBotApiService = leadslyBotApiService;
            _logger = logger;
        }

        private readonly LeadslyUserManager _userManager;
        private readonly IStripeRepository _stripeRepository;
        private readonly ILogger<Supervisor> _logger;
        private readonly ILeadslyBotApiService _leadslyBotApiService;

    }
}
