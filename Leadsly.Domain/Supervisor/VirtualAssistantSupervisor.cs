using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels.VirtualAssistant;
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
        private CloudMapDiscoveryServiceDTO CloudMapDiscoveryServiceDTO { get; set; }

        public async Task<VirtualAssistantInfoViewModel> GetVirtualAssistantInfoAsync(string userId, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.GetVirtualAssistantAsync(userId, ct);
            if (virtualAssistant == null)
            {
                return new VirtualAssistantInfoViewModel
                {
                    Created = false,
                    Assistant = null
                };
            }
            else
            {
                return new VirtualAssistantInfoViewModel
                {
                    Created = true,
                    Assistant = VirtualAssistantConverter.Convert(virtualAssistant)
                };
            }
        }

        public async Task<VirtualAssistantViewModel> CreateVirtualAssistantAsync(CreateVirtualAssistantRequest setup, CancellationToken ct = default)
        {
            LeadslyAccountSetupResult result = new();

            string halId = GenerateHalId();
            bool awsResourcesCreated = await CreateAwsResourcesAsync(halId, setup.UserId, result, ct);
            if (awsResourcesCreated == false)
            {
                result.Succeeded = false;
                return null;
            }

            EcsTaskDefinition newTaskDef = EcsTaskDefinitionConverter.Convert(EcsTaskDefinitionDTO);
            EcsService newService = EcsServiceConverter.Convert(EcsServiceDTO);
            CloudMapDiscoveryService newCloudMapService = CloudMapDiscoveryServiceConverter.Convert(CloudMapDiscoveryServiceDTO);

            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.CreateVirtualAssistantAsync(newTaskDef, newService, newCloudMapService, halId, setup.UserId, setup.TimeZoneId, ct);
            VirtualAssistantViewModel virtualAssistantViewModel = VirtualAssistantConverter.Convert(virtualAssistant);
            return virtualAssistantViewModel;
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
            CloudMapDiscoveryServiceDTO cloudMapServiceDiscoveryService = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(result, ct);
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
                result.Failure = new()
                {
                    Reason = "Ecs Tasks are not running. Tear down resources and try again"
                };
                await RollbackCloudResourcesAsync(userId, ct);
                return false;
            }

            // Healthcheck request to ensure all resources are running in the expected state
            bool healthCheck = await _cloudPlatformProvider.AreAwsResourcesHealthyAsync(cloudMapServiceDiscoveryService.Name, ct);
            if (healthCheck == false)
            {
                result.Succeeded = false;
                result.Failure = new()
                {
                    Reason = "Aws resources are not healthy"
                };
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
            await _cloudPlatformProvider.RollbackEcsServiceAsync(userId, ct);
            await _cloudPlatformProvider.RollbackCloudMapServiceAsync(userId, ct);
            await _cloudPlatformProvider.RolbackTaskDefinitionRegistrationAsync(userId, ct);
        }

        private string GenerateHalId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
