using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels.VirtualAssistant;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        private EcsService EcsService { get; set; }
        private EcsTaskDefinition EcsTaskDefinition { get; set; }
        private CloudMapDiscoveryService CloudMapDiscoveryService { get; set; }
        private IList<EcsTask> EcsServiceTasks { get; set; }

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

            // delete aws resources and their database counterparts (EcsServices, TaskDefinitions etc)
            await DeleteAwsResourcesAsync(virtualAssistant, userId, ct);

            if (virtualAssistant.SocialAccount != null)
            {
                SocialAccount socialAccount = await _socialAccountRepository.GetByIdAsync(virtualAssistant.SocialAccount.SocialAccountId, ct);

                // first delete connection withdraw phases
                await _campaignRepositoryFacade.DeleteConnectionWithdrawPhaseAsync(socialAccount.ConnectionWithdrawPhase.ConnectionWithdrawPhaseId, ct);

                // second delete Monitorfornewconnections phase
                await _campaignRepositoryFacade.DeleteMonitorForNewConnectionsPhaseAsync(socialAccount.MonitorForNewProspectsPhase.MonitorForNewConnectionsPhaseId, ct);

                // delete scan prospects for replies
                await _campaignRepositoryFacade.DeleteScanProspectsForRepliesPhaseAsync(socialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId, ct);

                await _socialAccountRepository.RemoveSocialAccountAsync(virtualAssistant.SocialAccount.SocialAccountId, ct);
            }

            await _virtualAssistantRepository.DeleteAsync(virtualAssistant.VirtualAssistantId, ct);

            viewModel.Succeeded = true;
            return viewModel;

        }

        private async Task DeleteAwsResourcesAsync(VirtualAssistant virtualAssistant, string userId, CancellationToken ct = default)
        {
            // if for whatever reason the ecs service is not set
            if (!string.IsNullOrEmpty(virtualAssistant.EcsService?.ServiceName) && !string.IsNullOrEmpty(virtualAssistant.EcsService?.ClusterArn))
            {
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, virtualAssistant.EcsService.ServiceName, virtualAssistant.EcsService.ClusterArn, ct);

                //if (!string.IsNullOrEmpty(virtualAssistant.CloudMapDiscoveryService?.ServiceDiscoveryId))
                //{
                //    await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, virtualAssistant.CloudMapDiscoveryService.ServiceDiscoveryId);
                //}

                // delete all ecs tasks associated with this ecs service
                await _cloudPlatformProvider.DeleteEcsTasksByEcsServiceId(virtualAssistant.EcsService.EcsServiceId, ct);

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

            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.CreateVirtualAssistantAsync(EcsTaskDefinition, EcsService, EcsServiceTasks, CloudMapDiscoveryService, halId, setup.UserId, setup.TimeZoneId, ct);
            VirtualAssistantViewModel virtualAssistantViewModel = VirtualAssistantConverter.Convert(virtualAssistant);
            return virtualAssistantViewModel;
        }

        private async Task<bool> CreateAwsResourcesAsync(string halId, string userId, CancellationToken ct = default)
        {
            // Register the new task definition
            string taskDefinition = $"{halId}-task-def";
            Amazon.ECS.Model.RegisterTaskDefinitionResponse ecsTaskDefinitionRegistrationResponse = await _cloudPlatformProvider.RegisterTaskDefinitionInAwsAsync(taskDefinition, halId, ct);
            if (ecsTaskDefinitionRegistrationResponse == null || ecsTaskDefinitionRegistrationResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to register ECS task definition in AWS");
                return false;
            }
            _logger.LogInformation("Successfully registered ECS task definition in AWS");

            // create cloud map service discovery service
            string serviceDiscoveryName = $"hal-{halId}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createCloudMapServiceResponse = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(serviceDiscoveryName, ct);
            if (createCloudMapServiceResponse == null || createCloudMapServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create Cloud Map service discovery service in AWS. HttpStatusCode: {createCloudMapServiceResponse?.HttpStatusCode}");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, serviceDiscoveryName, ct);
                return false;
            }
            _logger.LogInformation($"Successfully created Cloud Map service discovery service in AWS. HttpStatusCode: {createCloudMapServiceResponse?.HttpStatusCode}");

            // create ecs service
            string ecsServiceName = $"hal-{halId}-srv";
            Amazon.ECS.Model.CreateServiceResponse ecsCreateEcsServiceResponse = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsServiceName, ecsTaskDefinitionRegistrationResponse.TaskDefinition.Family, createCloudMapServiceResponse.Service.Arn, ct);
            if (ecsCreateEcsServiceResponse == null || ecsCreateEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create ECS Service in AWS");
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinitionRegistrationResponse.TaskDefinition.Family, ct);
                return false;
            }
            _logger.LogInformation("Successfully created ECS Service in AWS");

            // Ensure all aws resources are running before making healthcheck request
            bool running = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsCreateEcsServiceResponse.Service.ServiceName, ecsCreateEcsServiceResponse.Service.ClusterArn, ct);
            if (running == false)
            {
                _logger.LogError("Ecs Tasks are not running. Tear down resources and try again");
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateEcsServiceResponse.Service.ServiceName, ecsCreateEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinitionRegistrationResponse.TaskDefinition.Family, ct);
                return false;
            }

            IList<EcsTask> ecsServiceTasks = await _cloudPlatformProvider.ListEcsServiceTasksAsync(ecsCreateEcsServiceResponse.Service.ClusterArn, ecsCreateEcsServiceResponse.Service.ServiceArn, ct);
            if (ecsServiceTasks == null)
            {
                _logger.LogError("Failed to retreive ecs tasks for service {ecsServiceName}", ecsServiceName);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateEcsServiceResponse.Service.ServiceName, ecsCreateEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinitionRegistrationResponse.TaskDefinition.Family, ct);
                return false;
            }

            // I'm fairly certain healthcheck is already done by aws resources
            // Healthcheck request to ensure all resources are running in the expected state
            //bool healthCheck = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsServiceName, ecsCreateEcsServiceResponse.Service.ClusterArn, ct);
            //if (healthCheck == false)
            //{
            //    await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateEcsServiceResponse.Service.ServiceName, ecsCreateEcsServiceResponse.Service.ClusterArn, ct);
            //    await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createCloudMapServiceResponse.Service.Id, ct);
            //    await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinitionRegistrationResponse.TaskDefinition.Family, ct);                
            //    return false;
            //}

            EcsTaskDefinition = new()
            {
                TaskDefinitionArn = ecsTaskDefinitionRegistrationResponse.TaskDefinition.TaskDefinitionArn,
                Family = ecsTaskDefinitionRegistrationResponse.TaskDefinition.Family
            };

            CloudMapDiscoveryService = new()
            {
                Arn = createCloudMapServiceResponse.Service.Arn,
                CreateDate = createCloudMapServiceResponse.Service.CreateDate,
                Name = serviceDiscoveryName,
                ServiceDiscoveryId = createCloudMapServiceResponse.Service.Id
            };

            EcsService = new()
            {
                ClusterArn = ecsCreateEcsServiceResponse.Service.ClusterArn,
                CreatedAt = ((DateTimeOffset)ecsCreateEcsServiceResponse.Service.CreatedAt).ToUnixTimeSeconds(),
                CreatedBy = ecsCreateEcsServiceResponse.Service.CreatedBy,
                EcsServiceRegistries = ecsCreateEcsServiceResponse.Service.ServiceRegistries.Select(r => new EcsServiceRegistry()
                {
                    RegistryArn = r.RegistryArn,
                }).ToList(),
                SchedulingStrategy = ecsCreateEcsServiceResponse.Service.SchedulingStrategy,
                ServiceArn = ecsCreateEcsServiceResponse.Service.ServiceArn,
                ServiceName = ecsCreateEcsServiceResponse.Service.ServiceName,
                TaskDefinition = ecsCreateEcsServiceResponse.Service.TaskDefinition,
            };

            EcsServiceTasks = ecsServiceTasks;

            return true;
        }

        private string GenerateHalId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
