using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class ProvisionResourcesService : IProvisionResourcesService
    {
        public ProvisionResourcesService(
            ILogger<ProvisionResourcesService> logger,
            ICloudPlatformProvider cloudPlatformProvider,
            ICloudPlatformRepository cloudPlatformRepository)
        {
            _logger = logger;
            _cloudPlatformRepository = cloudPlatformRepository;
            _cloudPlatformProvider = cloudPlatformProvider;
        }

        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILogger<ProvisionResourcesService> _logger;
        private readonly ICloudPlatformProvider _cloudPlatformProvider;

        public IList<EcsService> EcsServices { get; private set; } = new List<EcsService>();
        public IList<EcsTaskDefinition> EcsTaskDefinitions { get; private set; } = new List<EcsTaskDefinition>();
        public IList<CloudMapDiscoveryService> CloudMapDiscoveryServices { get; private set; } = new List<CloudMapDiscoveryService>();

        public async Task<bool> CreateAwsTaskDefinitionsAsync(string halId, string userId, CancellationToken ct = default)
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

            EcsTaskDefinitions = new List<EcsTaskDefinition>();
            EcsTaskDefinition ecsGridTaskDefinition = new()
            {
                Purpose = EcsResourcePurpose.Grid,
                HalId = halId,
                TaskDefinitionArn = ecsGridTaskDefinitionRegistrationResponse.TaskDefinition.TaskDefinitionArn,
                Family = ecsGridTaskDefinitionRegistrationResponse.TaskDefinition.Family
            };
            EcsTaskDefinitions.Add(ecsGridTaskDefinition);

            EcsTaskDefinition ecsHalTaskDefinition = new()
            {
                Purpose = EcsResourcePurpose.Hal,
                HalId = halId,
                TaskDefinitionArn = ecsHalTaskDefinitionRegistrationResponse.TaskDefinition.TaskDefinitionArn,
                Family = ecsHalTaskDefinitionRegistrationResponse.TaskDefinition.Family
            };
            EcsTaskDefinitions.Add(ecsHalTaskDefinition);

            EcsTaskDefinition ecsProxyTaskDefinition = new()
            {
                Purpose = EcsResourcePurpose.Proxy,
                HalId = halId,
                TaskDefinitionArn = ecsProxyTaskDefinitionRegistrationResponse.TaskDefinition.TaskDefinitionArn,
                Family = ecsProxyTaskDefinitionRegistrationResponse.TaskDefinition.Family
            };
            EcsTaskDefinitions.Add(ecsProxyTaskDefinition);

            return true;
        }

        public async Task<bool> CreateAwsResourcesAsync(string halId, string userId, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            // create cloud map service discovery service for the grid
            string serviceGridDiscoveryName = $"grid-{halId}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createGridCloudMapServiceResponse = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(serviceGridDiscoveryName, configuration.ServiceDiscoveryConfig.Grid, ct);
            if (createGridCloudMapServiceResponse == null || createGridCloudMapServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create grid Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");
                await RollbackTaskDefinitionsAsync(userId, ct);
                return false;
            }

            CloudMapDiscoveryService gridCloudMapService = new()
            {
                Purpose = EcsResourcePurpose.Grid,
                HalId = halId,
                Arn = createGridCloudMapServiceResponse.Service.Arn,
                CreateDate = createGridCloudMapServiceResponse.Service.CreateDate,
                Name = serviceGridDiscoveryName,
                NamespaceId = configuration.ServiceDiscoveryConfig.Grid.NamespaceId,
                ServiceDiscoveryId = createGridCloudMapServiceResponse.Service.Id
            };
            CloudMapDiscoveryServices.Add(gridCloudMapService);

            _logger.LogInformation($"Successfully created grid Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");

            // create cloud map service discovery service for hal
            string serviceHalDiscoveryName = $"hal-{halId}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createHalCloudMapServiceResponse = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(serviceHalDiscoveryName, configuration.ServiceDiscoveryConfig.Hal, ct);
            if (createHalCloudMapServiceResponse == null || createHalCloudMapServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create hal Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                return false;
            }

            CloudMapDiscoveryService halCloudMapService = new()
            {
                Purpose = EcsResourcePurpose.Hal,
                HalId = halId,
                Arn = createHalCloudMapServiceResponse.Service.Arn,
                CreateDate = createHalCloudMapServiceResponse.Service.CreateDate,
                Name = serviceHalDiscoveryName,
                NamespaceId = configuration.ServiceDiscoveryConfig.Hal.NamespaceId,
                ServiceDiscoveryId = createHalCloudMapServiceResponse.Service.Id
            };
            CloudMapDiscoveryServices.Add(halCloudMapService);

            _logger.LogInformation($"Successfully created hal Cloud Map service discovery service in AWS. HttpStatusCode: {createGridCloudMapServiceResponse?.HttpStatusCode}");

            // create cloud map service discovery service for proxy
            string serviceProxyDiscoveryName = $"proxy-{halId}-srv-disc";
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createProxyCloudMapServiceResponse = await _cloudPlatformProvider.CreateCloudMapDiscoveryServiceInAwsAsync(serviceProxyDiscoveryName, configuration.ServiceDiscoveryConfig.Proxy, ct);
            if (createProxyCloudMapServiceResponse == null || createProxyCloudMapServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create hal Cloud Map service discovery service in AWS. HttpStatusCode: {createProxyCloudMapServiceResponse?.HttpStatusCode}");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                return false;
            }

            CloudMapDiscoveryService proxyCloudMapService = new()
            {
                Purpose = EcsResourcePurpose.Proxy,
                HalId = halId,
                Arn = createProxyCloudMapServiceResponse.Service.Arn,
                CreateDate = createProxyCloudMapServiceResponse.Service.CreateDate,
                Name = serviceProxyDiscoveryName,
                NamespaceId = configuration.ServiceDiscoveryConfig.Proxy.NamespaceId,
                ServiceDiscoveryId = createProxyCloudMapServiceResponse.Service.Id
            };
            CloudMapDiscoveryServices.Add(proxyCloudMapService);

            _logger.LogInformation($"Successfully created proxy Cloud Map service discovery service in AWS. HttpStatusCode: {createProxyCloudMapServiceResponse?.HttpStatusCode}");

            // create ecs service for grid
            string ecsGridServiceName = $"grid-{halId}-srv";
            string gridTaskDefinitionFamily = EcsTaskDefinitions.Where(t => t.Purpose == EcsResourcePurpose.Grid).FirstOrDefault()?.Family;
            if (string.IsNullOrEmpty(gridTaskDefinitionFamily) == true)
            {
                throw new Exception("No Task Definitions were found! You must create them first");
            }
            Amazon.ECS.Model.CreateServiceResponse ecsCreateGridEcsServiceResponse = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsGridServiceName, gridTaskDefinitionFamily, createGridCloudMapServiceResponse.Service.Arn, configuration.EcsServiceConfig.Grid, ct);
            if (ecsCreateGridEcsServiceResponse == null || ecsCreateGridEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create grid ECS Service in AWS");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                return false;
            }

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
                Purpose = EcsResourcePurpose.Grid,
                HalId = halId
            };
            ecsGridService.CloudMapDiscoveryService = gridCloudMapService;
            gridCloudMapService.EcsService = ecsGridService;
            EcsServices.Add(ecsGridService);

            _logger.LogInformation("Successfully created grid ECS Service in AWS");

            // create ecs service for hal
            string ecsHalServiceName = $"hal-{halId}-srv";
            string halTaskDefinitionFamily = EcsTaskDefinitions.Where(t => t.Purpose == EcsResourcePurpose.Hal).FirstOrDefault()?.Family;
            if (string.IsNullOrEmpty(halTaskDefinitionFamily) == true)
            {
                throw new Exception("No Task Definitions were found! You must create them first");
            }
            Amazon.ECS.Model.CreateServiceResponse ecsCreateHalEcsServiceResponse = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsHalServiceName, halTaskDefinitionFamily, createHalCloudMapServiceResponse.Service.Arn, configuration.EcsServiceConfig.Hal, ct);
            if (ecsCreateHalEcsServiceResponse == null || ecsCreateHalEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create hal ECS Service in AWS");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }

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
                Purpose = EcsResourcePurpose.Hal,
                HalId = halId
            };
            ecsHalService.CloudMapDiscoveryService = halCloudMapService;
            halCloudMapService.EcsService = ecsHalService;
            EcsServices.Add(ecsHalService);

            _logger.LogInformation("Successfully created hal ECS Service in AWS");

            string ecsProxyServiceName = $"proxy-{halId}-srv";
            string proxyTaskDefinitionFamily = EcsTaskDefinitions.Where(t => t.Purpose == EcsResourcePurpose.Proxy).FirstOrDefault()?.Family;
            if (string.IsNullOrEmpty(proxyTaskDefinitionFamily) == true)
            {
                throw new Exception("No Task Definitions were found! You must create them first");
            }
            Amazon.ECS.Model.CreateServiceResponse ecsCreateProxyEcsServiceResponse = await _cloudPlatformProvider.CreateEcsServiceInAwsAsync(ecsProxyServiceName, proxyTaskDefinitionFamily, createProxyCloudMapServiceResponse.Service.Arn, configuration.EcsServiceConfig.Hal, ct);
            if (ecsCreateProxyEcsServiceResponse == null || ecsCreateProxyEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create proxy ECS Service in AWS");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }

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
                Purpose = EcsResourcePurpose.Proxy,
                HalId = halId
            };
            ecsProxyService.CloudMapDiscoveryService = proxyCloudMapService;

            proxyCloudMapService.EcsService = ecsProxyService;
            EcsServices.Add(ecsProxyService);

            _logger.LogInformation("Successfully created hal ECS Service in AWS");

            // Ensure all aws resources are running before making healthcheck request
            bool ecsGridTasksRunning = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsCreateGridEcsServiceResponse.Service.ServiceName, ecsCreateGridEcsServiceResponse.Service.ClusterArn, ct);
            if (ecsGridTasksRunning == false)
            {
                _logger.LogError("Ecs Grid Tasks are not running. Tear down resources and try again");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }

            bool ecsHalTasksRunning = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsCreateHalEcsServiceResponse.Service.ServiceName, ecsCreateHalEcsServiceResponse.Service.ClusterArn, ct);
            if (ecsHalTasksRunning == false)
            {
                _logger.LogError("Ecs Hal Tasks are not running. Tear down resources and try again");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }

            bool ecsProxyTasksRunning = await _cloudPlatformProvider.EnsureEcsServiceTasksAreRunningAsync(ecsCreateProxyEcsServiceResponse.Service.ServiceName, ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ct);
            if (ecsHalTasksRunning == false)
            {
                _logger.LogError("Ecs Proxy Tasks are not running. Tear down resources and try again");
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }

            IList<EcsTask> ecsGridServiceTasks = await _cloudPlatformProvider.ListEcsServiceTasksAsync(ecsCreateGridEcsServiceResponse.Service.ClusterArn, ecsCreateGridEcsServiceResponse.Service.ServiceArn, ct);
            if (ecsGridServiceTasks == null)
            {
                _logger.LogError("Failed to retreive grid ecs tasks for service {ecsServiceName}", ecsGridServiceName);
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }
            ecsGridService.EcsTasks = ecsGridServiceTasks;

            IList<EcsTask> ecsHalServiceTasks = await _cloudPlatformProvider.ListEcsServiceTasksAsync(ecsCreateHalEcsServiceResponse.Service.ClusterArn, ecsCreateHalEcsServiceResponse.Service.ServiceArn, ct);
            if (ecsHalServiceTasks == null)
            {
                _logger.LogError("Failed to retreive hal ecs tasks for service {ecsServiceName}", ecsHalServiceName);
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }
            ecsHalService.EcsTasks = ecsHalServiceTasks;

            IList<EcsTask> ecsProxyServiceTasks = await _cloudPlatformProvider.ListEcsServiceTasksAsync(ecsCreateProxyEcsServiceResponse.Service.ClusterArn, ecsCreateProxyEcsServiceResponse.Service.ServiceArn, ct);
            if (ecsProxyServiceTasks == null)
            {
                _logger.LogError("Failed to retreive proxy ecs tasks for service {ecsServiceName}", ecsProxyServiceName);
                await RollbackTaskDefinitionsAsync(userId, ct);
                await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
                await RollbackEcsServicesAsync(userId, ct);
                return false;
            }
            ecsProxyService.EcsTasks = ecsProxyServiceTasks;

            return true;
        }

        private async Task RollbackTaskDefinitionsAsync(string userId, CancellationToken ct = default)
        {
            foreach (EcsTaskDefinition taskDefinition in EcsTaskDefinitions)
            {
                await _cloudPlatformProvider.DeleteAwsTaskDefinitionRegistrationAsync(userId, taskDefinition.Family, ct);
            }
        }

        private async Task RollbackEcsServicesAsync(string userId, CancellationToken ct = default)
        {
            foreach (EcsService ecsService in EcsServices)
            {
                await _cloudPlatformProvider.DeleteAwsEcsServiceAsync(userId, ecsService.ServiceName, ecsService.ClusterArn, ct);
            }
        }

        private async Task RollbackCloudMapDiscoveryServiceAsync(string userId, CancellationToken ct = default)
        {
            foreach (CloudMapDiscoveryService cloudMapService in CloudMapDiscoveryServices)
            {
                await _cloudPlatformProvider.DeleteAwsCloudMapServiceAsync(userId, cloudMapService.ServiceDiscoveryId, ct);
            }
        }

        public async Task RollbackAllResourcesAsync(string userId, CancellationToken ct = default)
        {
            await RollbackTaskDefinitionsAsync(userId, ct);
            await RollbackCloudMapDiscoveryServiceAsync(userId, ct);
            await RollbackEcsServicesAsync(userId, ct);
        }
    }
}
