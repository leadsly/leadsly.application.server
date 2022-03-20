using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class ProductingHostedService : IHostedService
    {
        public ProductingHostedService(IProducingService productingService)
        {
            _productinService = productingService;
        }

        private readonly IProducingService _productinService;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _productinService.StartRecurringJobs();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
