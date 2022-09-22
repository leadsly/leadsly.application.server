using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class ProducingHostedService : IHostedService
    {
        public ProducingHostedService(ILogger<ProducingHostedService> logger, IProducingService productingService, IOptions<FeatureFlagsOptions> options)
        {
            _featureFlagsOptions = options.Value;
            _productinService = productingService;
            _logger = logger;
        }

        private readonly FeatureFlagsOptions _featureFlagsOptions;
        private readonly IProducingService _productinService;
        private readonly ILogger<ProducingHostedService> _logger;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_featureFlagsOptions.AllInOneVirtualAssistant == true)
            {
                _logger.LogInformation("AllInOneVirtualAssistant feature flag is on");

                _logger.LogInformation("Executing AllInOneVirtualAssistant recurring jobs logic.");
                _productinService.StartRecurringJobs_AllInOneVirtualAssistant();
                _logger.LogInformation("Finished executing AllInOneVirtualAssistant recurring jobs logic.");
            }
            else
            {
                _logger.LogInformation("AllInOneVirtualAssistant feature flag is off");

                _logger.LogInformation("Executing StartAsync in ProducingHostedService");
                _productinService.StartRecurringJobs();
                _logger.LogInformation("Finished executing StartAsync in ProducingHostedService");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
