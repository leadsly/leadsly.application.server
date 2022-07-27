using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Leadsly.Domain.Services
{
    public class UrlService : IUrlService
    {
        public UrlService(ILogger<UrlService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        private readonly ILogger<UrlService> _logger;
        private readonly IWebHostEnvironment _env;

        public string GetHalsBaseUrl(string namespaceName, string serviceDiscName)
        {
            _logger.LogInformation("Getting hal's url");
            string url = string.Empty;
            if (_env.IsDevelopment())
            {
                string hostName = "localhost";
                long port = 5021;
                url = $"https://{hostName}:{port}/api";
            }
            else if (_env.IsStaging())
            {
                string hostName = "hal";
                url = $"http://{hostName}/api";
            }
            else
            {
                url = $"https://{serviceDiscName}.{namespaceName}/api";
            }

            _logger.LogDebug("Final url is {url}", url);
            return url;
        }
    }
}
