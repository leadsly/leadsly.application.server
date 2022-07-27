using Leadsly.Application.Model;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Leadsly.Domain.Services
{
    public class UrlService : IUrlService
    {
        public UrlService(ILogger<UrlService> logger, IWebHostEnvironment env, IOptions<HalConfigOptions> halConfigOptions)
        {
            _logger = logger;
            _env = env;
            _halConfigOptions = halConfigOptions.Value;
        }

        private readonly HalConfigOptions _halConfigOptions;
        private readonly ILogger<UrlService> _logger;
        private readonly IWebHostEnvironment _env;

        public string GetHalsBaseUrl(string namespaceName, string serviceDiscName)
        {
            _logger.LogInformation("Getting hal's url");
            string url = string.Empty;
            if (_env.IsDevelopment())
            {
                string hostName = _halConfigOptions.HostName;
                long port = _halConfigOptions.Port;
                url = $"https://{hostName}:{port}";
            }
            else if (_env.IsStaging())
            {
                string hostName = _halConfigOptions.HostName;
                url = $"http://{hostName}";
            }
            else
            {
                url = $"https://{serviceDiscName}.{namespaceName}";
            }

            _logger.LogDebug("Final url is {url}", url);
            return url;
        }
    }
}
