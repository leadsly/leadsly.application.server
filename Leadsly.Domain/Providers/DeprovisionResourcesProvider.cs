using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class DeprovisionResourcesProvider : IDeprovisionResourcesProvider
    {
        public DeprovisionResourcesProvider(
            ILogger<DeprovisionResourcesProvider> logger,
            IDeprovisionResourcesRepository repository,
            IDeprovisionResourcesService deprovisionService
            )
        {
            _logger = logger;
            _repository = repository;
            _deprovisionService = deprovisionService;
        }

        private readonly ILogger<DeprovisionResourcesProvider> _logger;
        private readonly IDeprovisionResourcesRepository _repository;
        private readonly IDeprovisionResourcesService _deprovisionService;

        public async Task DeprovisionResourcesAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogDebug("Deprovisioning resources for HalId {halId}", halId);

            VirtualAssistant virtualAssistant = await _repository.GetVirtualAssistantByHalIdAsync(halId, ct);

            await RemoveResourcesAsync(halId, virtualAssistant, EcsResourcePurpose.Grid);

            await RemoveResourcesAsync(halId, virtualAssistant, EcsResourcePurpose.Hal);

            virtualAssistant.Provisioned = false;
            if (await _repository.UpdateVirtualAssistantAsync(virtualAssistant) == null)
            {
                _logger.LogError("Failed to update {0} Deprovisioned property to true", nameof(VirtualAssistant));
            }
        }

        private async Task RemoveResourcesAsync(string halId, VirtualAssistant virtualAssistant, EcsResourcePurpose purpose, CancellationToken ct = default)
        {
            EcsService ecsServiceToRemove = virtualAssistant.EcsServices.Where(s => s.Purpose == purpose).FirstOrDefault();

            if (ecsServiceToRemove == null)
            {
                _logger.LogError("VirtualAssistant with id {0}, does not contain EcsService for purpose {1}", virtualAssistant.VirtualAssistantId, purpose);
                return;
            }

            if (await _deprovisionService.DeleteEcsServiceAsync(ecsServiceToRemove, ct) == false)
            {
                _logger.LogError("Failed to successfully de-provision ECS service. Prupose {0}. HalId {1}", purpose, halId);
            }
        }
    }
}
