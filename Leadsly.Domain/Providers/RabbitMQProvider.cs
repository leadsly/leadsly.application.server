using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class RabbitMQProvider : IRabbitMQProvider
    {
        public RabbitMQProvider(
            ILogger<RabbitMQProvider> logger, 
            IMemoryCache memoryCache,             
            IHalRepository halRepository,            
            ICloudPlatformRepository cloudPlatformRepository)
        {
            _logger = logger;
            _memoryCache = memoryCache;            
            _halRepository = halRepository;            
            _cloudPlatformRepository = cloudPlatformRepository;                        
        }

        private readonly ILogger<RabbitMQProvider> _logger;
        private readonly IHalRepository _halRepository;
        private readonly IMemoryCache _memoryCache;        
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        public async Task<string> CreateNewChromeProfileAsync(PhaseType phaseType, CancellationToken ct = default)
        {
            string phaseName = Enum.GetName(phaseType);
            _logger.LogDebug("There is no chrome profile entry created for {phaseName}. Creating a new chrome profile name", phaseName);
            // create the new chrome profile name and save it
            ChromeProfile profileName = new()
            {
                CampaignPhaseType = phaseType,
                Name = Guid.NewGuid().ToString()
            };
            await _halRepository.CreateChromeProfileAsync(profileName, ct);
            string chromeProfileName = profileName.Name;

            _logger.LogInformation("Using the following chrome profile name {chromeProfileName}", chromeProfileName);

            return chromeProfileName;
        }
        public CloudPlatformConfiguration GetCloudPlatformConfiguration()
        {
            if (_memoryCache.TryGetValue(CacheKeys.CloudPlatformConfigurationOptions, out CloudPlatformConfiguration config) == false)
            {
                _logger.LogDebug("Aws configuration options have not been loaded yet. Retrieving them from the database.");
                config = _cloudPlatformRepository.GetCloudPlatformConfiguration();
                _logger.LogDebug("Adding Aws configuration options to memory cache.");
                _memoryCache.Set(CacheKeys.CloudPlatformConfigurationOptions, config);
            }

            return config;
        }        
    }
}
