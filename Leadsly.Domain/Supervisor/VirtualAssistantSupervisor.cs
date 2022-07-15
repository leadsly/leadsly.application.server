﻿using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Entities;
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
        private EcsService EcsService { get; set; }
        private EcsTaskDefinition EcsTaskDefinition { get; set; }
        private CloudMapDiscoveryService CloudMapDiscoveryService { get; set; }

        public async Task<DeleteVirtualAssistantViewModel> DeleteVirtualAssistantAsync(string userId, CancellationToken ct = default)
        {
            DeleteVirtualAssistantViewModel viewModel = new DeleteVirtualAssistantViewModel
            {
                Succeeded = false
            };

            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.GetVirtualAssistantAsync(userId, ct);
            // even if it is falls send a success response to the client
            if (virtualAssistant == null)
            {
                _logger.LogWarning("Request to delete virtual assistant could not successfully complete because virtual assistant was not found.");
                return viewModel;
            }

            // if for whatever reason the ecs service is not set
            if (!string.IsNullOrEmpty(virtualAssistant.EcsService?.ServiceName) && !string.IsNullOrEmpty(virtualAssistant.EcsService?.ClusterArn))
            {
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, virtualAssistant.EcsService.ServiceName, virtualAssistant.EcsService.ClusterArn, ct);

                if (!string.IsNullOrEmpty(virtualAssistant.CloudMapDiscoveryService?.ServiceDiscoveryId))
                {
                    await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, virtualAssistant.CloudMapDiscoveryService.ServiceDiscoveryId);
                }

                // this will delete both ECS service and the CloudMapDiscoveryService from the database                
                await _cloudPlatformProvider.DeleteEcsServiceAsync(virtualAssistant.EcsService.EcsServiceId, ct);
            }

            // check if cloud map discovery service was set and delete it.
            if (!string.IsNullOrEmpty(virtualAssistant.CloudMapDiscoveryService?.ServiceDiscoveryId))
            {
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, virtualAssistant.CloudMapDiscoveryService.ServiceDiscoveryId);
                await _cloudPlatformProvider.DeleteCloudMapServiceAsync(virtualAssistant.CloudMapDiscoveryService.CloudMapDiscoveryServiceId);
            }

            if (!string.IsNullOrEmpty(virtualAssistant.EcsTaskDefinition?.Family))
            {
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, virtualAssistant.EcsTaskDefinition.Family, ct);
                await _cloudPlatformProvider.DeleteTaskDefinitionRegistrationAsync(virtualAssistant.EcsTaskDefinition.EcsTaskDefinitionId, ct);
            }

            await _cloudPlatformProvider.DeleteVirtualAssistantAsync(virtualAssistant.VirtualAssistantId, ct);

            viewModel.Succeeded = true;
            return viewModel;

        }

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
            string halId = GenerateHalId();
            bool awsResourcesCreated = await CreateAwsResourcesAsync(halId, setup.UserId, ct);
            if (awsResourcesCreated == false)
            {
                return null;
            }

            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.CreateVirtualAssistantAsync(EcsTaskDefinition, EcsService, CloudMapDiscoveryService, halId, setup.UserId, setup.TimeZoneId, ct);
            VirtualAssistantViewModel virtualAssistantViewModel = VirtualAssistantConverter.Convert(virtualAssistant);
            return virtualAssistantViewModel;
        }

        private async Task<bool> CreateAwsResourcesAsync(string halId, string userId, CancellationToken ct = default)
        {
            // Register the new task definition
            EcsTaskDefinition ecsTaskDefinition = await _cloudPlatformProvider.RegisterTaskDefinitionInAwsAsync(halId, ct);
            if (ecsTaskDefinition == null)
            {
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinition.Family, ct);
                return false;
            }

            // create cloud map service discovery service
            CloudMapDiscoveryService cloudMapServiceDiscoveryService = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(ct);
            if (cloudMapServiceDiscoveryService == null)
            {
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, cloudMapServiceDiscoveryService.Name, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinition.Family, ct);
                return false;
            }

            // create ecs service
            EcsService ecsService = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsTaskDefinition.Family, cloudMapServiceDiscoveryService.Arn, ct);
            if (ecsService == null)
            {
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsService.ServiceName, ecsService.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, cloudMapServiceDiscoveryService.ServiceDiscoveryId, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinition.Family, ct);
                return false;
            }

            // Ensure all aws resources are running before making healthcheck request
            bool running = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsService.ServiceName, ecsService.ClusterArn, ct);
            if (running == false)
            {
                _logger.LogError("Ecs Tasks are not running. Tear down resources and try again");
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsService.ServiceName, ecsService.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, cloudMapServiceDiscoveryService.ServiceDiscoveryId, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinition.Family, ct);
                return false;
            }

            // Healthcheck request to ensure all resources are running in the expected state
            //bool healthCheck = await _cloudPlatformProvider.AreAwsResourcesHealthyAsync(cloudMapServiceDiscoveryService.Name, ct);
            //if (healthCheck == false)
            //{
            //    result.Succeeded = false;
            //    result.Failure = new()
            //    {
            //        Reason = "Aws resources are not healthy"
            //    };
            //    await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsService.ServiceName, ecsService.ClusterArn, ct);
            //    await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, cloudMapServiceDiscoveryService.ServiceDiscoveryId, ct);
            //    await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinition.Family, ct);
            //    return false;
            //}

            EcsTaskDefinition = ecsTaskDefinition;
            CloudMapDiscoveryService = cloudMapServiceDiscoveryService;
            EcsService = ecsService;

            return true;
        }

        private string GenerateHalId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
