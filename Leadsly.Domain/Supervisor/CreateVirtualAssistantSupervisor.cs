using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.ViewModels.Cloud;
using Leadsly.Domain.Converters;
using Leadsly.Domain.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        private EcsServiceDTO EcsServiceDTO { get; set; }
        private EcsTaskDefinitionDTO EcsTaskDefinitionDTO { get; set; }
        private CloudMapServiceDiscoveryServiceDTO CloudMapDiscoveryServiceDTO { get; set; }

        private string GenerateHalId()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task<VirtualAssistantViewModel> CreateVirtualAssistantAsync(SetupAccountViewModel setup, LeadslyAccountSetupResult result, CancellationToken ct = default)
        {
            string halId = GenerateHalId();
            bool awsResourcesCreated = await CreateAwsResourcesAsync(halId, setup.UserId, result, ct);
            if (awsResourcesCreated == false)
            {
                result.Succeeded = false;
                return null;
            }

            VirtualAssistantViewModel virtualAssistantViewModel = VirtualAssistantConverter.Convert(await SaveVirtualAssistantAsync(halId, setup.UserId, setup.TimeZoneId, ct));
            return virtualAssistantViewModel;
        }

        private async Task<VirtualAssistant> SaveVirtualAssistantAsync(string halId, string userId, string timezoneId, CancellationToken ct = default)
        {
            EcsTaskDefinition newTaskDef = EcsTaskDefinitionConverter.Convert(EcsTaskDefinitionDTO);
            newTaskDef = await _cloudPlatformProvider.AddEcsTaskDefinitionAsync(newTaskDef, ct);

            EcsService newService = EcsServiceConverter.Convert(EcsServiceDTO);
            newService = await _cloudPlatformProvider.AddEcsServiceAsync(newService, ct);

            CloudMapDiscoveryService newCloudMapService = CloudMapServiceDiscoveryServiceConverter.Convert(CloudMapDiscoveryServiceDTO);
            newCloudMapService = await _cloudPlatformProvider.AddCloudMapDiscoveryServiceAsync(newCloudMapService, ct);

            HalUnit newHalUnit = new()
            {
                HalId = halId,
                TimeZoneId = timezoneId,
                ApplicationUserId = userId
            };
            await _halRepository.CreateAsync(newHalUnit);

            VirtualAssistant virtualAssistant = new VirtualAssistant
            {
                EcsTaskDefinition = newTaskDef,
                EcsService = newService,
                CloudMapDiscoveryService = newCloudMapService,
                ApplicationUserId = userId,
                HalId = halId
            };

            return await _virtualAssistantRepository.CreateAsync(virtualAssistant, ct);
        }

        private async Task<bool> CreateAwsResourcesAsync(string halId, string userId, LeadslyAccountSetupResult result, CancellationToken ct = default)
        {

            // Register the new task definition
            EcsTaskDefinitionDTO ecsTaskDefinition = await _cloudPlatformProvider.RegisterTaskDefinitionInAwsAsync(halId, result, ct);
            if (ecsTaskDefinition == null)
            {
                await RollbackCloudResourcesAsync(userId, ct);
                return false;
            }

            // create cloud map service discovery service
            CloudMapServiceDiscoveryServiceDTO cloudMapServiceDiscoveryService = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(result, ct);
            if (cloudMapServiceDiscoveryService == null)
            {
                await RollbackCloudResourcesAsync(userId, ct);
                return false;
            }

            // create ecs service
            EcsServiceDTO ecsService = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsTaskDefinition.Family, cloudMapServiceDiscoveryService.Arn, result, ct);
            if (ecsService == null)
            {
                await RollbackCloudResourcesAsync(userId, ct);
                return false;
            }

            // Ensure all aws resources are running before making healthcheck request
            bool running = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsService.ServiceName, ecsService.ClusterArn, ct);
            if (running == false)
            {
                _logger.LogError("Ecs Tasks are not running. Tear down resources and try again");
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Reason = "Ecs Tasks are not running. Tear down resources and try again"
                });
                await RollbackCloudResourcesAsync(userId, ct);
                return false;
            }

            // Healthcheck request to ensure all resources are running in the expected state
            bool healthCheck = await _cloudPlatformProvider.AreAwsResourcesHealthyAsync(cloudMapServiceDiscoveryService.Name, ct);
            if (healthCheck == false)
            {
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Reason = "Aws resources are not healthy"
                });
                await RollbackCloudResourcesAsync(userId, ct);
                return false;
            }

            EcsTaskDefinitionDTO = ecsTaskDefinition;
            CloudMapDiscoveryServiceDTO = cloudMapServiceDiscoveryService;
            EcsServiceDTO = ecsService;

            return true;
        }

        private async Task RollbackCloudResourcesAsync(string userId, CancellationToken ct)
        {
            await RollbackEcsServiceAsync(userId, ct);
            await RollbackCloudMapServiceAsync(userId, ct);
            await RolbackTaskDefinitionRegistrationAsync(userId, ct);
        }

        private async Task RolbackTaskDefinitionRegistrationAsync(string userId, CancellationToken ct = default)
        {
            // if task definition family name is returned, this means that the resource was not successfully removed.
            string taskDefinitionFamilyName = await _cloudPlatformProvider.RolbackTaskDefinitionRegistrationAsync(ct);
            if (taskDefinitionFamilyName != string.Empty)
            {
                _logger.LogWarning("Deregister operation for ecs task definition failed.");
                OrphanedCloudResource orphanedCloudResource = new()
                {
                    UserId = userId,
                    FriendlyName = "Task definition",
                    ResourceId = taskDefinitionFamilyName
                };

                _logger.LogInformation("Adding task definition to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
            }
        }

        private async Task RollbackCloudMapServiceAsync(string userId, CancellationToken ct = default)
        {
            // if task definition family name is returned, this means that the resource was not successfully removed.
            string taskDefinitionFamilyName = await _cloudPlatformProvider.RollbackCloudMapServiceAsync(ct);
            if (taskDefinitionFamilyName != string.Empty)
            {
                _logger.LogWarning("Operation to remove aws cloud map discovery service failed.");
                OrphanedCloudResource orphanedCloudResource = new()
                {
                    UserId = userId,
                    FriendlyName = "Cloud Map Discovery Service",
                    ResourceId = taskDefinitionFamilyName
                };

                _logger.LogInformation("Adding cloud map discovery service to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
            }
        }

        private async Task RollbackEcsServiceAsync(string userId, CancellationToken ct = default)
        {
            // if task definition family name is returned, this means that the resource was not successfully removed.
            string taskDefinitionFamilyName = await _cloudPlatformProvider.RollbackEcsServiceAsync(ct);
            if (taskDefinitionFamilyName != string.Empty)
            {
                _logger.LogWarning("Delete operation for ecs service failed.");
                OrphanedCloudResource orphanedCloudResource = new()
                {
                    UserId = userId,
                    FriendlyName = "Task definition",
                    ResourceId = taskDefinitionFamilyName
                };

                _logger.LogInformation("Adding ecs service to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
            }
        }
    }
}
