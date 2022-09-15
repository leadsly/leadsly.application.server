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
        private IList<EcsService> EcsServices { get; set; }
        private IList<EcsTaskDefinition> EcsTaskDefinitions { get; set; }
        private IList<CloudMapDiscoveryService> CloudMapDiscoveryServices { get; set; }
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

                // second delete Monitorfornewconnections phase
                await _campaignRepositoryFacade.DeleteMonitorForNewConnectionsPhaseAsync(socialAccount.MonitorForNewProspectsPhase.MonitorForNewConnectionsPhaseId, ct);

                // delete scan prospects for replies
                await _campaignRepositoryFacade.DeleteScanProspectsForRepliesPhaseAsync(socialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId, ct);

                await _socialAccountRepository.RemoveSocialAccountAsync(virtualAssistant.SocialAccount.SocialAccountId, ct);
            }

            await _halRepository.DeleteAsync(virtualAssistant.HalId, ct);

            await _virtualAssistantRepository.DeleteAsync(virtualAssistant.VirtualAssistantId, ct);

            viewModel.Succeeded = true;
            return viewModel;

        }

        private async Task DeleteAwsResourcesAsync(VirtualAssistant virtualAssistant, string userId, CancellationToken ct = default)
        {
            // if for whatever reason the ecs service is not set
            if (virtualAssistant.EcsServices != null || virtualAssistant.EcsServices.Count != 0)
            {
                foreach (EcsService ecsService in virtualAssistant.EcsServices.ToList())
                {
                    await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsService.ServiceName, ecsService.ClusterArn, ct);

                    // delete all ecs tasks associated with this ecs service
                    await _cloudPlatformProvider.DeleteEcsTasksByEcsServiceId(ecsService.EcsServiceId, ct);

                    // this will delete both ECS service and the CloudMapDiscoveryService from the database                
                    await _cloudPlatformProvider.DeleteEcsServiceAsync(ecsService.EcsServiceId, ct);
                }
            }

            // check if cloud map discovery service was set and delete it.
            if (virtualAssistant.CloudMapDiscoveryServices != null || virtualAssistant.CloudMapDiscoveryServices.Count != 0)
            {
                foreach (CloudMapDiscoveryService cloudMapService in virtualAssistant.CloudMapDiscoveryServices.ToList())
                {
                    await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, cloudMapService.ServiceDiscoveryId);
                    await _cloudPlatformProvider.DeleteCloudMapServiceAsync(cloudMapService.CloudMapDiscoveryServiceId);
                }
            }

            if (virtualAssistant.EcsTaskDefinitions != null || virtualAssistant.EcsTaskDefinitions.Count != 0)
            {
                foreach (EcsTaskDefinition ecsTaskDefinition in virtualAssistant.EcsTaskDefinitions.ToList())
                {
                    await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, ecsTaskDefinition.Family, ct);
                    await _cloudPlatformProvider.DeleteTaskDefinitionRegistrationAsync(ecsTaskDefinition.EcsTaskDefinitionId, ct);
                }
            }

            // remove S3 directory
            await _cloudPlatformProvider.DeleteAwsS3HalDirectoryAsync(virtualAssistant.HalId, ct);
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

            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.CreateVirtualAssistantAsync(EcsTaskDefinitions, EcsServices, EcsServiceTasks, CloudMapDiscoveryServices, halId, setup.UserId, setup.TimeZoneId, ct);
            VirtualAssistantViewModel virtualAssistantViewModel = VirtualAssistantConverter.Convert(virtualAssistant);
            return virtualAssistantViewModel;
        }

        private async Task<bool> CreateAwsResourcesAsync(string halId, string userId, CancellationToken ct = default)
        {
            // Register the new task definition
            string gridTaskDefinition = $"{halId}-grid-task-def";
            Amazon.ECS.Model.RegisterTaskDefinitionResponse ecsGridTaskDefinitionRegistrationResponse = await _cloudPlatformProvider.RegisterGridTaskDefinitionInAwsAsync(gridTaskDefinition, halId, ct);
            if (ecsGridTaskDefinitionRegistrationResponse == null || ecsGridTaskDefinitionRegistrationResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to register grid ECS task definition in AWS");
                return false;
            }
            _logger.LogInformation("Successfully registered grid ECS task definition in AWS");

            string halTaskDefinition = $"{halId}-hal-task-def";
            Amazon.ECS.Model.RegisterTaskDefinitionResponse ecsHalTaskDefinitionRegistrationResponse = await _cloudPlatformProvider.RegisterHalTaskDefinitionInAwsAsync(halTaskDefinition, halId, ct);
            if (ecsHalTaskDefinitionRegistrationResponse == null || ecsHalTaskDefinitionRegistrationResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to register hal ECS task definition in AWS");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                return false;
            }
            _logger.LogInformation("Successfully registered hal ECS task definition in AWS");

            string proxyTaskDefinition = $"{halId}-proxy-task-def";
            Amazon.ECS.Model.RegisterTaskDefinitionResponse ecsProxyTaskDefinitionRegistrationResponse = await _cloudPlatformProvider.RegisterProxyTaskDefinitionInAwsAsync(proxyTaskDefinition, halId, ct);
            if (ecsProxyTaskDefinitionRegistrationResponse == null || ecsProxyTaskDefinitionRegistrationResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to register proxy ECS task definition in AWS");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                return false;
            }
            _logger.LogInformation("Successfully registered proxy ECS task definition in AWS");

            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            // create cloud map service discovery service for the grid
            string serviceGridDiscoveryName = $"grid-{halId}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createGridCloudMapServiceResponse = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(serviceGridDiscoveryName, configuration.ServiceDiscoveryConfig.Grid, ct);
            if (createGridCloudMapServiceResponse == null || createGridCloudMapServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create grid Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                return false;
            }
            _logger.LogInformation($"Successfully created grid Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");

            // create cloud map service discovery service for hal
            string serviceHalDiscoveryName = $"hal-{halId}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createHalCloudMapServiceResponse = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(serviceHalDiscoveryName, configuration.ServiceDiscoveryConfig.Hal, ct);
            if (createHalCloudMapServiceResponse == null || createHalCloudMapServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create hal Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                return false;
            }
            _logger.LogInformation($"Successfully created hal Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");

            // create cloud map service discovery service for proxy
            string serviceProxyDiscoveryName = $"proxy-{halId}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createProxyCloudMapServiceResponse = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(serviceProxyDiscoveryName, configuration.ServiceDiscoveryConfig.Proxy, ct);
            if (createProxyCloudMapServiceResponse == null || createProxyCloudMapServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create hal Cloud Map service discovery service in AWS. HttpStatusCode: {createProxyCloudMapServiceResponse?.HttpStatusCode}");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                return false;
            }
            _logger.LogInformation($"Successfully created proxy Cloud Map service discovery service in AWS. HttpStatusCode: {createProxyCloudMapServiceResponse?.HttpStatusCode}");

            // create ecs service for grid
            string ecsGridServiceName = $"grid-{halId}-srv";
            Amazon.ECS.Model.CreateServiceResponse ecsCreateGridEcsServiceResponse = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsGridServiceName, ecsGridTaskDefinitionRegistrationResponse.TaskDefinition.Family, createGridCloudMapServiceResponse.Service.Arn, configuration.EcsServiceConfig.Grid, ct);
            if (ecsCreateGridEcsServiceResponse == null || ecsCreateGridEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create grid ECS Service in AWS");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                return false;
            }
            _logger.LogInformation("Successfully created grid ECS Service in AWS");

            // create ecs service for hal
            string ecsHalServiceName = $"hal-{halId}-srv";
            Amazon.ECS.Model.CreateServiceResponse ecsCreateHalEcsServiceResponse = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsHalServiceName, ecsHalTaskDefinitionRegistrationResponse.TaskDefinition.Family, createHalCloudMapServiceResponse.Service.Arn, configuration.EcsServiceConfig.Hal, ct);
            if (ecsCreateHalEcsServiceResponse == null || ecsCreateHalEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create hal ECS Service in AWS");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }
            _logger.LogInformation("Successfully created hal ECS Service in AWS");

            string ecsProxyServiceName = $"proxy-{halId}-srv";
            Amazon.ECS.Model.CreateServiceResponse ecsCreateProxyEcsServiceResponse = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsProxyServiceName, ecsProxyTaskDefinitionRegistrationResponse.TaskDefinition.Family, createProxyCloudMapServiceResponse.Service.Arn, configuration.EcsServiceConfig.Hal, ct);
            if (ecsCreateProxyEcsServiceResponse == null || ecsCreateProxyEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create proxy ECS Service in AWS");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }
            _logger.LogInformation("Successfully created hal ECS Service in AWS");

            // Ensure all aws resources are running before making healthcheck request
            bool ecsGridTasksRunning = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
            if (ecsGridTasksRunning == false)
            {
                _logger.LogError("Ecs Grid Tasks are not running. Tear down resources and try again");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }

            bool ecsHalTasksRunning = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
            if (ecsHalTasksRunning == false)
            {
                _logger.LogError("Ecs Hal Tasks are not running. Tear down resources and try again");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }

            bool ecsProxyTasksRunning = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
            if (ecsHalTasksRunning == false)
            {
                _logger.LogError("Ecs Proxy Tasks are not running. Tear down resources and try again");
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }

            IList<EcsTask> ecsGridServiceTasks = await _cloudPlatformProvider.ListEcsServiceTasksAsync(ecsCreateGridEcsServiceResponse.Service.ClusterArn, ecsCreateGridEcsServiceResponse.Service.ServiceArn, ct);
            if (ecsGridServiceTasks == null)
            {
                _logger.LogError("Failed to retreive grid ecs tasks for service {ecsServiceName}", ecsGridServiceName);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }

            IList<EcsTask> ecsHalServiceTasks = await _cloudPlatformProvider.ListEcsServiceTasksAsync(ecsCreateHalEcsServiceResponse.Service.ClusterArn, ecsCreateHalEcsServiceResponse.Service.ServiceArn, ct);
            if (ecsHalServiceTasks == null)
            {
                _logger.LogError("Failed to retreive hal ecs tasks for service {ecsServiceName}", ecsHalServiceName);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }

            IList<EcsTask> ecsProxyServiceTasks = await _cloudPlatformProvider.ListEcsServiceTasksAsync(ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ecsCreateProxyEcsServiceResponse.Service.ServiceArn, ct);
            if (ecsProxyServiceTasks == null)
            {
                _logger.LogError("Failed to retreive proxy ecs tasks for service {ecsServiceName}", ecsProxyServiceName);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, gridTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, halTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, proxyTaskDefinition, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createGridCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createHalCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, createProxyCloudMapServiceResponse.Service.Id, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
                return false;
            }

            EcsTaskDefinitions = new List<EcsTaskDefinition>();
            EcsTaskDefinition ecsGridTaskDefinition = new()
            {
                TaskDefinitionArn = ecsGridTaskDefinitionRegistrationResponse.TaskDefinition.TaskDefinitionArn,
                Family = ecsGridTaskDefinitionRegistrationResponse.TaskDefinition.Family
            };
            EcsTaskDefinitions.Add(ecsGridTaskDefinition);

            EcsTaskDefinition ecsHalTaskDefinition = new()
            {
                TaskDefinitionArn = ecsHalTaskDefinitionRegistrationResponse.TaskDefinition.TaskDefinitionArn,
                Family = ecsHalTaskDefinitionRegistrationResponse.TaskDefinition.Family
            };
            EcsTaskDefinitions.Add(ecsHalTaskDefinition);

            EcsTaskDefinition ecsProxyTaskDefinition = new()
            {
                TaskDefinitionArn = ecsProxyTaskDefinitionRegistrationResponse.TaskDefinition.TaskDefinitionArn,
                Family = ecsProxyTaskDefinitionRegistrationResponse.TaskDefinition.Family
            };
            EcsTaskDefinitions.Add(ecsProxyTaskDefinition);

            CloudMapDiscoveryServices = new List<CloudMapDiscoveryService>();
            CloudMapDiscoveryService gridCloudMapService = new()
            {
                Arn = createGridCloudMapServiceResponse.Service.Arn,
                CreateDate = createGridCloudMapServiceResponse.Service.CreateDate,
                Name = serviceGridDiscoveryName,
                NamespaceId = configuration.ServiceDiscoveryConfig.Grid.NamespaceId,
                ServiceDiscoveryId = createGridCloudMapServiceResponse.Service.Id
            };
            CloudMapDiscoveryServices.Add(gridCloudMapService);

            CloudMapDiscoveryService halCloudMapService = new()
            {
                Arn = createHalCloudMapServiceResponse.Service.Arn,
                CreateDate = createHalCloudMapServiceResponse.Service.CreateDate,
                Name = serviceHalDiscoveryName,
                NamespaceId = configuration.ServiceDiscoveryConfig.Hal.NamespaceId,
                ServiceDiscoveryId = createHalCloudMapServiceResponse.Service.Id
            };
            CloudMapDiscoveryServices.Add(halCloudMapService);

            CloudMapDiscoveryService proxyCloudMapService = new()
            {
                Arn = createProxyCloudMapServiceResponse.Service.Arn,
                CreateDate = createProxyCloudMapServiceResponse.Service.CreateDate,
                Name = serviceProxyDiscoveryName,
                NamespaceId = configuration.ServiceDiscoveryConfig.Proxy.NamespaceId,
                ServiceDiscoveryId = createProxyCloudMapServiceResponse.Service.Id
            };
            CloudMapDiscoveryServices.Add(proxyCloudMapService);

            EcsServices = new List<EcsService>();
            EcsService ecsGridService = new()
            {
                ClusterArn = ecsCreateGridEcsServiceResponse.Service.ClusterArn,
                CreatedAt = ((DateTimeOffset)ecsCreateGridEcsServiceResponse.Service.CreatedAt).ToUnixTimeSeconds(),
                CreatedBy = ecsCreateGridEcsServiceResponse.Service.CreatedBy,
                EcsServiceRegistries = ecsCreateGridEcsServiceResponse.Service.ServiceRegistries.Select(r => new EcsServiceRegistry()
                {
                    RegistryArn = r.RegistryArn,
                }).ToList(),
                SchedulingStrategy = ecsCreateGridEcsServiceResponse.Service.SchedulingStrategy,
                ServiceArn = ecsCreateGridEcsServiceResponse.Service.ServiceArn,
                ServiceName = ecsCreateGridEcsServiceResponse.Service.ServiceName,
                TaskDefinition = ecsCreateGridEcsServiceResponse.Service.TaskDefinition,
                Purpose = Purpose.Grid
            };
            ecsGridService.CloudMapDiscoveryService = gridCloudMapService;
            ecsGridService.EcsTasks = ecsGridServiceTasks;
            gridCloudMapService.EcsService = ecsGridService;
            EcsServices.Add(ecsGridService);

            EcsService ecsHalService = new()
            {
                ClusterArn = ecsCreateHalEcsServiceResponse.Service.ClusterArn,
                CreatedAt = ((DateTimeOffset)ecsCreateHalEcsServiceResponse.Service.CreatedAt).ToUnixTimeSeconds(),
                CreatedBy = ecsCreateHalEcsServiceResponse.Service.CreatedBy,
                EcsServiceRegistries = ecsCreateHalEcsServiceResponse.Service.ServiceRegistries.Select(r => new EcsServiceRegistry()
                {
                    RegistryArn = r.RegistryArn,
                }).ToList(),
                SchedulingStrategy = ecsCreateHalEcsServiceResponse.Service.SchedulingStrategy,
                ServiceArn = ecsCreateHalEcsServiceResponse.Service.ServiceArn,
                ServiceName = ecsCreateHalEcsServiceResponse.Service.ServiceName,
                TaskDefinition = ecsCreateHalEcsServiceResponse.Service.TaskDefinition,
                Purpose = Purpose.Hal
            };
            ecsHalService.CloudMapDiscoveryService = halCloudMapService;
            ecsHalService.EcsTasks = ecsHalServiceTasks;
            halCloudMapService.EcsService = ecsHalService;
            EcsServices.Add(ecsHalService);

            EcsService ecsProxyService = new()
            {
                ClusterArn = ecsCreateProxyEcsServiceResponse.Service.ClusterArn,
                CreatedAt = ((DateTimeOffset)ecsCreateProxyEcsServiceResponse.Service.CreatedAt).ToUnixTimeSeconds(),
                CreatedBy = ecsCreateProxyEcsServiceResponse.Service.CreatedBy,
                EcsServiceRegistries = ecsCreateProxyEcsServiceResponse.Service.ServiceRegistries.Select(r => new EcsServiceRegistry()
                {
                    RegistryArn = r.RegistryArn,
                }).ToList(),
                SchedulingStrategy = ecsCreateProxyEcsServiceResponse.Service.SchedulingStrategy,
                ServiceArn = ecsCreateProxyEcsServiceResponse.Service.ServiceArn,
                ServiceName = ecsCreateProxyEcsServiceResponse.Service.ServiceName,
                TaskDefinition = ecsCreateProxyEcsServiceResponse.Service.TaskDefinition,
                Purpose = Purpose.Proxy
            };
            ecsProxyService.CloudMapDiscoveryService = proxyCloudMapService;
            ecsProxyService.EcsTasks = ecsProxyServiceTasks;
            proxyCloudMapService.EcsService = ecsProxyService;
            EcsServices.Add(ecsProxyService);

            EcsServiceTasks = ecsGridServiceTasks.Concat(ecsHalServiceTasks).Concat(ecsProxyServiceTasks).ToList();

            return true;
        }

        private string GenerateHalId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
