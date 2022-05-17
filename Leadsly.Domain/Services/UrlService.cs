using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class UrlService : IUrlService
    {
        public UrlService(ILogger<UrlService> logger, IWebHostEnvironment env, ICloudPlatformRepository cloudPlatformRepository, IOptions<HalConfigOptions> halConfigOptions)
        {
            _logger = logger;
            _env = env;
            _cloudPlatformRepository = cloudPlatformRepository;
            _halConfigOptions = halConfigOptions.Value;
        }

        private readonly HalConfigOptions _halConfigOptions;
        private readonly ILogger<UrlService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;        

        public string GetHalsBaseUrl(string namespaceName)
        {
            _logger.LogInformation("Getting hal's url");
            string url = string.Empty;
            if (_env.IsDevelopment())
            {
                string hostName = _halConfigOptions.HostName;
                long port = _halConfigOptions.Port;
                url = $"http://{hostName}:{port}";
            }
            else if (_env.IsStaging())
            {
                string hostName = _halConfigOptions.HostName;
                url = $"http://{hostName}";
            }
            else
            {
                CloudPlatformConfiguration config = _cloudPlatformRepository.GetCloudPlatformConfiguration();
                string serviceDiscoveryName = config.ServiceDiscoveryConfig.Name;
                url = $"https://{serviceDiscoveryName}.{namespaceName}";
            }

            _logger.LogDebug("Final url is {url}", url);
            return url;
        }
    }
}
