using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class DeleteResourcesService : IDeleteResourcesService
    {
        public DeleteResourcesService(
            ILogger<DeleteResourcesService> logger,
            ICloudPlatformRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        private readonly ILogger<DeleteResourcesService> _logger;
        private readonly ICloudPlatformRepository _repository;

        public async Task<bool> DeleteCloudMapServiceAsync(string cloudMapServiceId, CancellationToken ct = default)
        {
            await _repository.RemoveCloudMapServiceDiscoveryServiceAsync(cloudMapServiceId, ct);
        }

        public async Task<bool> DeleteEcsServiceAsync(string ecsServiceId, CancellationToken ct = default)
        {
            return await _repository.RemoveEcsServiceAsync(ecsServiceId, ct);
        }
    }
}
