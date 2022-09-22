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
            IDeprovisionResourcesService deprovisionService,
            IDeleteResourcesService deleteService,
            IVirtualAssistantRepository repository
            )
        {
            _repository = repository;
            _logger = logger;
            _deprovisionService = deprovisionService;
            _deleteService = deleteService;
        }

        private readonly IVirtualAssistantRepository _repository;
        private readonly ILogger<DeprovisionResourcesProvider> _logger;
        private readonly IDeprovisionResourcesService _deprovisionService;
        private readonly IDeleteResourcesService _deleteService;

        public async Task DeprovisionResourcesAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogDebug("Deprovisioning resources for HalId {halId}", halId);

            VirtualAssistant virtualAssistant = await _repository.GetByHalIdAsync(halId, ct);

            await RemoveResourcesAsync(halId, virtualAssistant, EcsResourcePurpose.Grid);

            await RemoveResourcesAsync(halId, virtualAssistant, EcsResourcePurpose.Hal);
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

            // may not be necessary, deprovisioning ecs service may already delete all tasks
            //if (await _deprovisionService.StopAllEcsTasksAsync(serviceToRemove.EcsTasks, serviceToRemove.ClusterArn, purpose))
            //{

            //}

            if (await _deprovisionService.DeleteCloudMapServiceAsync(ecsServiceToRemove.CloudMapDiscoveryService.ServiceDiscoveryId, ct) == false)
            {
                _logger.LogError("Failed to successfully delete Cloud Map Discovery service. Prupose {0}. HalId {1}", purpose, halId);
            }

            if (await _deleteService.DeleteEcsServiceAsync(ecsServiceToRemove.EcsServiceId) == false)
            {

            }

            CloudMapDiscoveryService cloudMapServiceToRemove = virtualAssistant.CloudMapDiscoveryServices.Where(s => s.Purpose == purpose).FirstOrDefault();

            if (cloudMapServiceToRemove == null)
            {
                _logger.LogError("VirtualAssistant with id {0}, does not contain CloudMapService for purpose {1}", virtualAssistant.VirtualAssistantId, purpose);
                return;
            }

            if (await _deleteService.DeleteCloudMapServiceAsync(cloudMapServiceToRemove.CloudMapDiscoveryServiceId, ct) == false)
            {
                _logger.LogError("Failed to successfully delete cloud map service from the database. Purpose {0}. HalId {1}", purpose, halId);
            }
        }
    }
}
