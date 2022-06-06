using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class ProducingHostedService : IHostedService
    {
        public ProducingHostedService(ILogger<ProducingHostedService> logger, IProducingService productingService)
        {
            _productinService = productingService;
            _logger = logger;
        }

        private readonly IProducingService _productinService;
        private readonly ILogger<ProducingHostedService> _logger;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing StartAsync in ProducingHostedService");
            _productinService.StartRecurringJobs();
            _logger.LogInformation("Finished executing StartAsync in ProducingHostedService");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
