using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Models;
using Leadsly.Models.Aws;
using Leadsly.Models.Aws.ElasticContainerService;
using Leadsly.Models.Aws.ServiceDiscovery;
using Leadsly.Models.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CloudPlatformProvider : ICloudPlatformProvider
    {
        public CloudPlatformProvider(IAwsElasticContainerService awsElasticContainerService, ICloudPlatformRepository cloudPlatformRepository, IAwsServiceDiscoveryService awsServiceDiscoveryService, ILogger<CloudPlatformProvider> logger)
        {
            _awsElasticContainerService = awsElasticContainerService;                        
            _awsServiceDiscoveryService = awsServiceDiscoveryService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _logger = logger;
        }

        private readonly IAwsElasticContainerService _awsElasticContainerService;          
        private readonly IAwsServiceDiscoveryService _awsServiceDiscoveryService;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILogger<CloudPlatformProvider> _logger;

        public async Task<NewSocialAccountSetupResult> SetupNewContainerForUserSocialAccountAsync(CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            if(configuration == null)
            {
                _logger.LogError("Failed to retrieve aws ecs service, ecs task definition and service discovery configuration.");
                NewSocialAccountSetupResult result = new()
                {
                    Succeeded = false,
                    CreateEcsServiceSucceeded = false,
                    CreateServiceDiscoveryServiceSucceeded = false,
                    CreateEcsTaskDefinitionSucceeded = false                    
                };
                result.Failures.Add(new()
                {
                    Detail = "Cannot get ecs service, ecs task definition or service discovery required configuration",
                    Reason = "Failed to retrieve configuration details"
                });
                return result;
            }

            string taskDefinition = $"{Guid.NewGuid()}";
            NewContainerSetupDTO userSetup = new()
            {
                EcsTaskDefinition = new()
                {
                    // name of the hal container
                    ContainerName = $"hal-{Guid.NewGuid()}-container",
                    Family = taskDefinition
                },
                CloudMapServiceDiscovery = new()
                {
                    // name used to discover this service by in the future
                    Name = $"hal-{Guid.NewGuid()}"
                },
                EcsService = new()
                {
                    ClusterArn = configuration.EcsServiceConfig.ClusterArn,
                    ServiceName = $"hal-{Guid.NewGuid()}-service",
                    TaskDefinition = taskDefinition
                }
            };

            return await SetupNewContainerAsync(userSetup, configuration, ct);
        }

        public async Task<CloudPlatformOperationResult> SetupExistingContainerAsync(SetupExistingUserInLeadslyDTO createContainer, CancellationToken ct = default)
        {

            var containerInfoDTO = new CloudPlatformOperationResult();
            return containerInfoDTO;
        }

        public async Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, CancellationToken ct = default)
        {
            _logger.LogInformation("Aws resource cleanup started.");
            NewContainerSetupDTO rollbackDetails = setupToRollback.Value;
            // first start removing service discovery service
            if (setupToRollback.CreateServiceDiscoveryServiceSucceeded)
            {
                _logger.LogInformation("Starting to delete service discovery service.");
                if(rollbackDetails.EcsService.Registries.Count() > 1)
                {
                    _logger.LogWarning("Retrieving service discovery service arn from ecs service registries list. Expected to see a single entry in the list but more were detected! Using first value from the list");
                }                
                await DeleteServiceDiscoveryServiceAsync(rollbackDetails.CloudMapServiceDiscovery, ct);
                _logger.LogInformation("Finished deleting service discovery service.");
            }

            // then ecs service
            if (setupToRollback.CreateEcsServiceSucceeded)
            {
                _logger.LogInformation("Starting to delete ecs service.");
                await DeleteEcsServiceAsync(rollbackDetails.EcsService, ct);
                _logger.LogInformation("Finished deleting ecs service.");
            }

            // then deregister task definition            
            if (setupToRollback.CreateEcsTaskDefinitionSucceeded)
            {
                _logger.LogInformation("Starting to deregister task definition.");
                // since this is an immediate rollback revision will always be 1. For regular deletes this information will have to be fetched from the api.
                rollbackDetails.EcsTaskDefinition.Family = $"{rollbackDetails.EcsTaskDefinition.Family}:1";
                await DeregisterTaskDefinitionAsync(rollbackDetails.EcsTaskDefinition, ct);
                _logger.LogInformation("Finished deregistering task definition.");
            }

            _logger.LogInformation("Aws resource cleanup completed.");
        }

        private async Task<NewSocialAccountSetupResult> SetupNewContainerAsync(NewContainerSetupDTO userSetup, CloudPlatformConfiguration configuration, CancellationToken ct = default)
        {
            NewSocialAccountSetupResult result = new()
            {
                Succeeded = false,
                CreateEcsServiceSucceeded = false,
                CreateServiceDiscoveryServiceSucceeded = false,
                CreateEcsTaskDefinitionSucceeded = false,
                Value = userSetup
            };

            // Register a new task for the user                  
            Amazon.ECS.Model.RegisterTaskDefinitionResponse registerTaskDefinitionResponse = await RegisterTaskDefinitionAsync(userSetup.EcsTaskDefinition, configuration.EcsTaskDefinitionConfig, ct);

            if (registerTaskDefinitionResponse == null || registerTaskDefinitionResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Detail = "Response was null or did not equal expected OK 200",
                    Reason = "Failed to register new task definition"
                });
                return result;
            }
            result.CreateEcsTaskDefinitionSucceeded = true;

            // Create new AWS Cloud Map Service Discovery Service for this container
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createDiscoveryServiceResponse = await CreateServiceDiscoveryServiceAsync(userSetup.CloudMapServiceDiscovery, configuration.ServiceDiscoveryConfig, ct);

            if (createDiscoveryServiceResponse == null || createDiscoveryServiceResponse.Service == null || createDiscoveryServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Detail = "Response was null or did not equal expected OK 200",
                    Reason = "Failed to create new service discovery service"
                });
                return result;
            }
            result.CreateServiceDiscoveryServiceSucceeded = true;
            // update userSetup with the service discovery id. In case something fails, this will id will be used to clean up aws resources
            userSetup.CloudMapServiceDiscovery.Id = createDiscoveryServiceResponse.Service.Id;

            // link users ecs service with the created service discovery service
            userSetup.EcsService.Registries = new()
            {
                new()
                {
                    RegistryArn = createDiscoveryServiceResponse.Service.Arn
                }
            };

            // Create new Ecs Service Task with the task definition and assign it to the created service discovery service
            Amazon.ECS.Model.CreateServiceResponse createEcsServiceResponse = await CreateEcsServiceAsync(userSetup.EcsService, configuration.EcsServiceConfig, ct);

            if (createEcsServiceResponse == null || createEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Detail = "Response was null or did not equal expected OK 200",
                    Reason = "Failed to create new ecs service"
                });
                return result;
            }

            result.CreateEcsServiceSucceeded = true;
            // add the upated userSetup here
            result.Value = userSetup;

            CloudPlatformOperationResult ensureServiceTasksAreRunningResult = await EnsureEcsServiceTasksAreRunningAsync(userSetup, configuration, ct);
            if (ensureServiceTasksAreRunningResult.Succeeded == false)
            {
                result.Failures = ensureServiceTasksAreRunningResult.Failures;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private async Task<CloudPlatformOperationResult> EnsureEcsServiceTasksAreRunningAsync(NewContainerSetupDTO userSetup, CloudPlatformConfiguration configuration, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                // Check elapsed time w/o stopping/resetting the stopwatch                
                if (stopwatch.Elapsed.Seconds >= 10)
                {
                    // At least 5 seconds elapsed, restart stopwatch.
                    stopwatch.Stop();
                    result = await AreEcsServiceTasksRunningAsync(userSetup, configuration, ct);
                    if (result.Succeeded)
                    {
                        if (((bool)result.Value) == true)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // if an error occured break out of the function
                        break;
                    }
                    stopwatch.Start();
                }
            }

            return result;
        }

        private async Task<CloudPlatformOperationResult> AreEcsServiceTasksRunningAsync(NewContainerSetupDTO userSetup, CloudPlatformConfiguration configuration, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            Amazon.ECS.Model.DescribeServicesResponse response = await DescribeServicesAsync(userSetup.EcsService, configuration.EcsServiceConfig, ct);

            if (response == null || response.Services == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Value = false;
                result.Failures.Add(new()
                {
                    Arn = userSetup.EcsService.ServiceName,
                    Detail = "Aws API returned null, services were null or status code did not equal 200",
                    Reason = "Failed to get number of pending tasks for this service"
                });
                return result;
            }

            if (response.Services.Count() > 1)
            {
                _logger.LogWarning("Expected to find a single service in the response but more than one were found. Using first service in the collection to check if all tasks are running");
            }

            result.Succeeded = true;
            result.Value = response.Services.First().PendingCount == 0;
            return result;
        }

        private async Task DeleteServiceDiscoveryServiceAsync(CloudMapServiceDiscoveryDTO serviceDiscovery, CancellationToken ct = default)
        {
            DeleteServiceDiscoveryServiceRequest request = new()
            {
                Id = serviceDiscovery.Id
            };

            Amazon.ServiceDiscovery.Model.DeleteServiceResponse response = await _awsServiceDiscoveryService.DeleteServiceAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Delete operation for service discovery service failed.");
                // ideally here, we log the resources information into a database for manual clean up
            }
        }

        private async Task DeleteEcsServiceAsync(EcsServiceDTO ecsServiceDto, CancellationToken ct = default)
        {
            DeleteEcsServiceRequest request = new()
            {
                Cluster = ecsServiceDto.ClusterArn,
                Force = true,
                Service = ecsServiceDto.ServiceName
            };

            Amazon.ECS.Model.DeleteServiceResponse response = await _awsElasticContainerService.DeleteServiceAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Delete operation for ecs service failed.");
                // ideally here, we log the resources information into a database for manual clean up
            }
        }

        private async Task DeregisterTaskDefinitionAsync(EcsTaskDefinitionDTO taskDefinition, CancellationToken ct = default)
        {
            DeregisterEcsTaskDefinitionRequest request = new()
            {
                TaskDefinition = taskDefinition.Family
            };

            Amazon.ECS.Model.DeregisterTaskDefinitionResponse response = await _awsElasticContainerService.DeregisterTaskDefinitionAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Deregister operation for ecs task definition failed.");
                // ideally here, we log the resources information into a database for manual clean up
            }
        }

        private async Task<Amazon.ECS.Model.CreateServiceResponse> CreateEcsServiceAsync(EcsServiceDTO ecsService, EcsServiceConfig config, CancellationToken ct = default)
        {
            CreateEcsServiceRequest createEcsServiceRequest = new()
            {
                AssignPublicIp = config.AssignPublicIp,
                Cluster = config.ClusterArn,
                DesiredCount = config.DesiredCount,
                LaunchType = config.LaunchType,
                SchedulingStrategy = config.SchedulingStrategy,
                SecurityGroups = config.SecurityGroups,
                Subnets = config.Subnets,
                TaskDefinition = ecsService.TaskDefinition,
                ServiceName = ecsService.ServiceName,
                EcsServiceRegistries = ecsService.Registries.Select(r => new EcsServiceRegistry
                {
                    RegistryArn = r.RegistryArn
                }).ToList()
            };

            return await _awsElasticContainerService.CreateServiceAsync(createEcsServiceRequest, ct);
        }

        private async Task<Amazon.ServiceDiscovery.Model.CreateServiceResponse> CreateServiceDiscoveryServiceAsync(CloudMapServiceDiscoveryDTO cloudMapServiceDiscovery, CloudMapServiceDiscoveryConfig config, CancellationToken ct = default)
        {
            CreateServiceDiscoveryServiceRequest createServiceDiscoveryServiceRequest = new()
            {
                Name = cloudMapServiceDiscovery.Name,
                NamespaceId = config.NamespaceId,
                DnsConfig = new()
                {                    
                    CloudMapDnsRecords = new()
                    {
                        new() 
                        {
                            TTL = config.DnsRecordTTL,
                            Type = config.DnsRecordType
                        }
                    }
                }
            };

            return await _awsServiceDiscoveryService.CreateServiceAsync(createServiceDiscoveryServiceRequest, ct);
        }

        private async Task<Amazon.ECS.Model.RegisterTaskDefinitionResponse> RegisterTaskDefinitionAsync(EcsTaskDefinitionDTO taskDefinition, EcsTaskDefinitionConfig config, CancellationToken ct = default)
        {
            RegisterEcsTaskDefinitionRequest registerEcsTaskDefinitionRequest = new()
            {
                Cpu = config.Cpu,
                Family = taskDefinition.Family,
                EcsContainerDefinitions = config.ContainerDefinitions.Select(c => new EcsContainerDefinition
                { 
                    Image = c.Image,
                    Name = taskDefinition.ContainerName,
                    PortMappings = c.PortMappings.Select(p => new EcsPortMapping
                    {
                        ContainerPort = p.ContainerPort,
                        Protocol = p.Protocol,
                    }).ToList()                    
                }).ToList(),
                ExecutionRoleArn = config.ExecutionRoleArn,
                Memory = config.Memory,
                NetworkMode = config.NetworkMode,
                RequiresCompatibilities = config.RequiresCompatibilities,
            };

            return await _awsElasticContainerService.RegisterTaskDefinitionAsync(registerEcsTaskDefinitionRequest, ct);
        }

        private async Task<Amazon.ECS.Model.RunTaskResponse> RunTaskAsync(EcsTaskDTO ecsTaskOptions, EcsTaskConfig config, CancellationToken ct = default)
        {
            RunEcsTaskRequest request = new()
            {
                AssignPublicIp = config.AssignPublicIp,
                ClusterArn = config.ClusterArn,
                Count = config.Count,
                LaunchType = config.LaunchType,
                Subnets = config.Subnets,
                TaskDefinition = config.TaskDefinition                
            };

            return await _awsElasticContainerService.RunTaskAsync(request, ct);

        }

        private async Task<Amazon.ECS.Model.DescribeServicesResponse> DescribeServicesAsync(EcsServiceDTO userService, EcsServiceConfig config, CancellationToken ct = default)
        {
            List<string> services = new() { userService.ServiceName };
            string cluster = config.ClusterArn;

            DescribeEcsServicesRequest request = new()
            {
                Cluster = cluster,
                Services = services
            };

            return await _awsElasticContainerService.DescribeServicesAsync(request, ct);
        }

        private async Task<Amazon.ECS.Model.UpdateServiceResponse> UpdateServiceAsync(EcsServiceDTO service, EcsServiceConfig config, CancellationToken ct = default)
        {
            UpdateEcsServiceRequest request = new()
            {
                ClusterArn = config.ClusterArn,
                DesiredCount = service.DesiredCount,
                ServiceName = config.ServiceName
            };

            return await _awsElasticContainerService.UpdateServiceAsync(request, ct);
        }
    }
}
