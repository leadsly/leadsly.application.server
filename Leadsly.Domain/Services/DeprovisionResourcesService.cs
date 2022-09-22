using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class DeprovisionResourcesService : IDeprovisionResourcesService
    {
        public DeprovisionResourcesService(
            ILogger<DeprovisionResourcesService> logger,
            IAwsElasticContainerService awsElasticContainerService,
            IAwsServiceDiscoveryService awsServiceDiscoveryService)
        {
            _awsElasticContainerService = awsElasticContainerService;
            _awsServiceDiscoveryService = awsServiceDiscoveryService;
            _logger = logger;
        }

        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly IAwsServiceDiscoveryService _awsServiceDiscoveryService;
        private readonly ILogger<DeprovisionResourcesService> _logger;

        public async Task<bool> DeleteEcsServiceAsync(EcsService serviceToRemove, CancellationToken ct = default)
        {
            if (serviceToRemove == null)
            {
                _logger.LogError("Cannot delete ecs service because it is null.");
                return false;
            }

            if (serviceToRemove.ServiceName == null)
            {
                _logger.LogError("EcsService does not have service name set");
                return false;
            }

            if (serviceToRemove.ClusterArn == null)
            {
                _logger.LogError("EcsService does not have cluster arn set");
                return false;
            }

            return await _awsElasticContainerService.DeleteServiceAsync(serviceToRemove.ServiceName, serviceToRemove.ClusterArn, ct);
        }

        public Task<bool> StopAllEcsTasksAsync(IEnumerable<EcsTask> ecsTasks, string cluster, EcsResourcePurpose purpose, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteCloudMapServiceAsync(string serviceDiscoveryId, CancellationToken ct = default)
        {
            return await _awsServiceDiscoveryService.DeleteCloudMapDiscoveryServiceAsync(serviceDiscoveryId, ct);
        }
    }
}
