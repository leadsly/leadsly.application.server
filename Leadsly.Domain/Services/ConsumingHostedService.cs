using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class ConsumingHostedService : IHostedService
    {
        public ConsumingHostedService(IConsumingService consumingService)
        {
            _consumingService = consumingService;
        }

        private readonly IConsumingService _consumingService;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _consumingService.StartConsumingAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
