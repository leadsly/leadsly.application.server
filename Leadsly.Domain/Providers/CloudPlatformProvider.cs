using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Aws.ElasticContainerService;
using Leadsly.Application.Model.Aws.Route53;
using Leadsly.Application.Model.Aws.ServiceDiscovery;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CloudPlatformProvider : ICloudPlatformProvider
    {
        public CloudPlatformProvider(
            IAwsElasticContainerService awsElasticContainerService,
            ICloudPlatformRepository cloudPlatformRepository,
            IAwsServiceDiscoveryService awsServiceDiscoveryService,
            IAwsRoute53Service awsRoute53Service,
            ILeadslyHalApiService leadslyBotApiService,
            IHalRepository halRepository,
            IOrphanedCloudResourcesRepository orphanedCloudResourcesRepository,
            ILogger<CloudPlatformProvider> logger)
        {
            _awsElasticContainerService = awsElasticContainerService;
            _awsServiceDiscoveryService = awsServiceDiscoveryService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _orphanedCloudResourcesRepository = orphanedCloudResourcesRepository;
            _awsRoute53Service = awsRoute53Service;
            _leadslyBotApiService = leadslyBotApiService;
            _halRepository = halRepository;
            _logger = logger;
        }

        private readonly ILeadslyHalApiService _leadslyBotApiService;
        private readonly IHalRepository _halRepository;
        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly IAwsRoute53Service _awsRoute53Service;
        private readonly IAwsServiceDiscoveryService _awsServiceDiscoveryService;
        private readonly IOrphanedCloudResourcesRepository _orphanedCloudResourcesRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILogger<CloudPlatformProvider> _logger;
        private readonly int DefaultTimeToWaitForEcsServicePendingTasks_InSeconds = 120;
        private readonly string HealthCheckEndpoint = "api/healthcheck";

        public async Task RollbackEcsServiceAsync(string userId, CancellationToken ct = default)
        {
            string resource = await _awsElasticContainerService.RollbackServiceAsync(ct);
            if (resource != string.Empty)
            {
                _logger.LogDebug("Delete operation for ecs service failed. Adding ecs service to orphaned cloud resources table for manual clean up.");
                await SaveOrphanedResourcesAsync(userId, resource, ct);
            }
        }

        public async Task RollbackCloudMapServiceAsync(string userId, CancellationToken ct = default)
        {
            string resource = await _awsElasticContainerService.RollbackTaskDefinitionRegistrationAsync(ct);
            if (resource != string.Empty)
            {
                _logger.LogDebug("Operation to remove aws cloud map discovery service failed. Adding cloud map discovery service to orphaned cloud resources table for manual clean up.");
                await SaveOrphanedResourcesAsync(userId, resource, ct);
            }
        }

        public async Task RolbackTaskDefinitionRegistrationAsync(string userId, CancellationToken ct = default)
        {
            string resource = await _awsServiceDiscoveryService.RollbackCloudMapDiscoveryServiceAsync(ct);
            if (resource != string.Empty)
            {
                _logger.LogDebug("Deregister operation for ecs task definition failed. Adding task definition to orphaned cloud resources table for manual clean up.");
                await SaveOrphanedResourcesAsync(userId, resource, ct);
            }
        }

        private async Task SaveOrphanedResourcesAsync(string userId, string resource, CancellationToken ct = default)
        {
            OrphanedCloudResource orphanedCloudResource = new()
            {
                UserId = userId,
                FriendlyName = "Task definition",
                ResourceId = resource
            };

            await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
        }

        public async Task<EcsServiceDTO> CreateEcsServiceInAwsAsync(string taskDefinition, string cloudMapServiceArn, LeadslyAccountSetupResult result, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            EcsServiceDTO ecsService = CreateAwsEcsService(configuration, taskDefinition, cloudMapServiceArn);

            Amazon.ECS.Model.CreateServiceResponse createEcsServiceResponse = await CreateEcsServiceAsync(ecsService, ct);
            if (createEcsServiceResponse == null || createEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create ECS Service in AWS");
                result.Succeeded = false;
                result.Failure = new()
                {
                    Reason = "Failed to create ECS Service in AWS"
                };
                return null;
            }
            else
            {
                _logger.LogInformation("Successfully created ECS Service in AWS");
                ecsService.ServiceArn = createEcsServiceResponse.Service.ServiceArn;
                ecsService.CreatedAt = createEcsServiceResponse.Service.CreatedAt;
                ecsService.CreatedBy = createEcsServiceResponse.Service.CreatedBy;
            }

            return ecsService;
        }

        private EcsServiceDTO CreateAwsEcsService(CloudPlatformConfiguration configuration, string taskDefinition, string cloudMapServiceArn)
        {
            return new EcsServiceDTO
            {
                ClusterArn = configuration.EcsServiceConfig.ClusterArn,
                ServiceName = $"hal-{Guid.NewGuid()}-service",
                TaskDefinition = taskDefinition,
                // UserId = userId,
                AssignPublicIp = configuration.EcsServiceConfig.AssignPublicIp,
                DesiredCount = configuration.EcsServiceConfig.DesiredCount,
                SchedulingStrategy = configuration.EcsServiceConfig.SchedulingStrategy,
                LaunchType = configuration.EcsServiceConfig.LaunchType,
                Subnets = configuration.EcsServiceConfig.Subnets,
                SecurityGroups = configuration.EcsServiceConfig.SecurityGroups,
                Registries = new()
                {
                    new()
                    {
                        RegistryArn = cloudMapServiceArn
                    }
                }
            };
        }

        public async Task<CloudMapDiscoveryServiceDTO> CreateCloudMapDiscoveryServiceInAwsAsync(LeadslyAccountSetupResult result, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            CloudMapDiscoveryServiceDTO cloudMapServiceDiscoveryDTO = CreateAwsCloudMapService(configuration);

            Amazon.ServiceDiscovery.Model.CreateServiceResponse createDiscoveryServiceResponse = await CreateServiceDiscoveryServiceAsync(cloudMapServiceDiscoveryDTO, ct);
            if (createDiscoveryServiceResponse == null || createDiscoveryServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Failed to create Cloud Map service discovery service in AWS. HttpStatusCode: {createDiscoveryServiceResponse?.HttpStatusCode}");
                result.Succeeded = false;
                result.Failure = new()
                {
                    Reason = "Failed to create Cloud Map service discovery service in AWS"
                };
                return null;
            }
            else
            {
                _logger.LogInformation($"Successfully created Cloud Map service discovery service in AWS. HttpStatusCode: {createDiscoveryServiceResponse?.HttpStatusCode}");
                cloudMapServiceDiscoveryDTO.ServiceDiscoveryId = createDiscoveryServiceResponse.Service.Id;
                cloudMapServiceDiscoveryDTO.Arn = createDiscoveryServiceResponse.Service.Arn;
                cloudMapServiceDiscoveryDTO.CreateDate = createDiscoveryServiceResponse.Service.CreateDate;
                cloudMapServiceDiscoveryDTO.Description = createDiscoveryServiceResponse.Service.Description;
                cloudMapServiceDiscoveryDTO.CreateRequestId = createDiscoveryServiceResponse.Service.CreatorRequestId;
            }

            return cloudMapServiceDiscoveryDTO;
        }

        private CloudMapDiscoveryServiceDTO CreateAwsCloudMapService(CloudPlatformConfiguration configuration)
        {
            return new CloudMapDiscoveryServiceDTO
            {
                // name used to discover this service by in the future
                Name = $"hal-{Guid.NewGuid()}-srv-disc",
                NamespaceId = configuration.ServiceDiscoveryConfig.NamespaceId,
                DnsConfig = new()
                {
                    CloudMapDnsRecords = new()
                            {
                                new()
                                {
                                    TTL = configuration.ServiceDiscoveryConfig.DnsRecordTTL,
                                    Type = configuration.ServiceDiscoveryConfig.DnsRecordType
                                }
                            }
                }
            };
        }

        public async Task<EcsTaskDefinitionDTO> RegisterTaskDefinitionInAwsAsync(string halId, LeadslyAccountSetupResult result, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            // Register the new task definition
            EcsTaskDefinitionDTO ecsTaskDefinition = CreateAwsTaskDefinition(halId, configuration);
            Amazon.ECS.Model.RegisterTaskDefinitionResponse registerTaskDefinitionResponse = await RegisterTaskDefinitionAsync(ecsTaskDefinition, ct);
            if (registerTaskDefinitionResponse == null || registerTaskDefinitionResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to register ECS task definition in AWS");
                result.Succeeded = false;
                result.Failure = new()
                {
                    Reason = "Failed to register ECS task definition in AWS"
                };

                return null;
            }
            else
            {
                _logger.LogInformation("Successfully registered ECS task definition in AWS.");
                ecsTaskDefinition.TaskDefinitionArn = registerTaskDefinitionResponse.TaskDefinition.TaskDefinitionArn;
            }

            return ecsTaskDefinition;
        }

        private EcsTaskDefinitionDTO CreateAwsTaskDefinition(string halId, CloudPlatformConfiguration configuration)
        {
            string taskDefinition = $"{Guid.NewGuid()}-task-def";
            string containerName = $"hal-{Guid.NewGuid()}-container";
            return new EcsTaskDefinitionDTO
            {
                ContainerName = containerName,
                Family = taskDefinition,
                Cpu = configuration.EcsTaskDefinitionConfig.Cpu,
                ContainerDefinitions = configuration.EcsTaskDefinitionConfig.ContainerDefinitions.Select(c => new ContainerDefinitionDTO
                {
                    Image = c.Image,
                    Name = taskDefinition,
                    EnviornmentVariables = new()
                            {
                                new KeyValuePair<string, string>(Enum.GetName(DockerEnvironmentVariables.HAL_ID), halId)
                            }
                }).ToList(),
                ExecutionRoleArn = configuration.EcsTaskDefinitionConfig.ExecutionRoleArn,
                TaskRoleArn = configuration.EcsTaskDefinitionConfig.TaskRoleArn,
                Memory = configuration.EcsTaskDefinitionConfig.Memory,
                NetworkMode = configuration.EcsTaskDefinitionConfig.NetworkMode,
                RequiresCompatibilities = configuration.EcsTaskDefinitionConfig.RequiresCompatibilities
            };
        }

        public async Task<NewSocialAccountSetupResult> SetupNewCloudResourceForUserSocialAccountAsync(string userId, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            if (configuration == null)
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
                    Code = Codes.CONFIGURATION_DATA_MISSING,
                    Detail = "Cannot get ecs service, ecs task definition or service discovery required configuration",
                    Reason = "Failed to retrieve configuration details"
                });
                return result;
            }

            SocialAccountCloudResourceDTO userSetup = SetSocialAccountCloudResourceData(configuration, userId);

            if (userSetup == null)
            {
                NewSocialAccountSetupResult result = new()
                {
                    Succeeded = false,
                    CreateEcsServiceSucceeded = false,
                    CreateServiceDiscoveryServiceSucceeded = false,
                    CreateEcsTaskDefinitionSucceeded = false
                };
                result.Failures.Add(new()
                {
                    Code = Codes.OBJECT_MAPPING,
                    Detail = "Failed to generate users social account resource data class",
                    Reason = "Failed to perform data mapping"
                });
                return result;
            }

            return await SetupNewContainerAsync(userSetup, ct);
        }
        public async Task<ExistingSocialAccountSetupResultDTO> ConnectToExistingCloudResourceAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResultDTO result = new()
            {
                Succeeded = false
            };

            DescribeEcsServiceResponse describeEcsServiceResponseResult = await GetEcsServiceDetailsAsync(socialAccount, ct);
            if (describeEcsServiceResponseResult.Succeeded == false)
            {
                result.Failures = describeEcsServiceResponseResult.Failures;
                return result;
            }

            result = await ValidateEcsServiceForSocialAccountAsync(describeEcsServiceResponseResult.Service, socialAccount, ct);
            if (result.Succeeded == false)
            {
                return result;
            }

            // perform initial healthcheck to hal via service discovery service name
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            // perform health check on hal
            //HalHealthCheckResponse healthCheckResponse = await RunHalsHealthCheckAsync(socialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService.Name, configuration, ct);
            //// its possible that the failure occured because dns record has stale ip address
            //if (healthCheckResponse.Succeeded == false)
            //{
            //    _logger.LogInformation("Healthcheck using discovery service name failed. Trying to run health check with private ip address.");
            //    result = await PerformHalsHealthCheckWithPrivateIpAsync(configuration.ServiceDiscoveryConfig, socialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService, ct);
            //    if (result.Succeeded == false)
            //    {
            //        return result;
            //    }
            //}

            //// in the very slim chance that this ip address is pointing to a recycled ip address that is running a different version of hal lets check the container names
            //if(healthCheckResponse.Value.HalId != socialAccount.SocialAccountCloudResource.HalId)
            //{
            //    _logger.LogWarning("Rare edge case occured. Hal's health check successfully responded but the name of hal running in the container is different then what user has registered to their name. Requring DNS to get fresh private ip address.");
            //    result = await PerformHalsHealthCheckWithPrivateIpAsync(configuration.ServiceDiscoveryConfig, socialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService, ct);
            //    if(result.Succeeded == false)
            //    {
            //        return result;
            //    }
            //}

            result.Succeeded = true;
            return result;
        }

        public async Task<bool> AreAwsResourcesHealthyAsync(string cloudMapServiceName, CancellationToken ct = default)
        {
            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();

            HalHealthCheckResponse healthCheckResponse = await RunHalsHealthCheckAsync(cloudMapServiceName, configuration, ct);
            if (healthCheckResponse.Succeeded == false)
            {
                _logger.LogInformation("Running initial hals health check did not succeeded");
                return false;
            }
            return true;
        }

        private async Task<HalHealthCheckResponse> RunHalsHealthCheckAsync(string serviceDiscoveryName, CloudPlatformConfiguration configuration, CancellationToken ct = default)
        {
            HealthCheckRequest halHealthCheckRequest = new()
            {
                ServiceDiscoveryName = serviceDiscoveryName,
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                RequestUrl = HealthCheckEndpoint
            };

            return await PerformHalsHealthCheckAsync(halHealthCheckRequest, ct);
        }

        public async Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Aws resource cleanup started.");
            SocialAccountCloudResourceDTO rollbackDetails = setupToRollback.Value;

            // first delete ecs service
            if (setupToRollback.CreateEcsServiceSucceeded)
            {
                _logger.LogInformation("Starting to delete ecs service.");
                await DeleteEcsServiceAsync(rollbackDetails.EcsService, userId, ct);
                _logger.LogInformation("Finished deleting ecs service.");
            }

            // second start removing service discovery service
            if (setupToRollback.CreateServiceDiscoveryServiceSucceeded)
            {
                _logger.LogInformation("Starting to delete service discovery service.");
                if (rollbackDetails.EcsService.Registries.Count() > 1)
                {
                    _logger.LogWarning("Retrieving service discovery service arn from ecs service registries list. Expected to see a single entry in the list but more were detected! Using first value from the list");
                }
                await DeleteServiceDiscoveryServiceAsync(rollbackDetails.CloudMapServiceDiscovery, userId, ct);
                _logger.LogInformation("Finished deleting service discovery service.");
            }

            // then deregister task definition            
            if (setupToRollback.CreateEcsTaskDefinitionSucceeded)
            {
                _logger.LogInformation("Starting to deregister task definition.");
                // since this is an immediate rollback revision will always be 1. For regular deletes this information will have to be fetched from the api.
                rollbackDetails.EcsTaskDefinition.Family = $"{rollbackDetails.EcsTaskDefinition.Family}:1";
                await DeregisterTaskDefinitionAsync(rollbackDetails.EcsTaskDefinition, userId, ct);
                _logger.LogInformation("Finished deregistering task definition.");
            }

            _logger.LogInformation("Aws resource cleanup completed.");
        }
        public async Task RemoveUsersSocialAccountCloudResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            _logger.LogInformation("Aws resource cleanup started.");
            SocialAccountCloudResource awsCloudDetailsToRemove = socialAccount.SocialAccountCloudResource;

            //////////////////////////////// ORDER MATTERS
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////
            /// First delete ecs service
            _logger.LogInformation("Starting to delete ecs service.");
            await DeleteEcsServiceAsync(awsCloudDetailsToRemove.EcsService, socialAccount.UserId, ct);
            _logger.LogInformation("Finished deleting ecs service.");

            // then Service Discovery Service
            _logger.LogInformation("Starting to delete service discovery service.");
            //await DeleteServiceDiscoveryServiceAsync(awsCloudDetailsToRemove.CloudMapServiceDiscoveryService, socialAccount.UserId, ct);
            _logger.LogInformation("Finished deleting service discovery service.");

            // then deregister task definition            
            _logger.LogInformation("Starting to deregister task definition.");

            // might need to be refactored IF task definition has revisions
            awsCloudDetailsToRemove.EcsTaskDefinition.Family = $"{awsCloudDetailsToRemove.EcsTaskDefinition.Family}:1";
            await DeregisterTaskDefinitionAsync(awsCloudDetailsToRemove.EcsTaskDefinition, socialAccount.UserId, ct);
            _logger.LogInformation("Finished deregistering task definition.");

            _logger.LogInformation("Aws resource cleanup completed.");
        }
        private SocialAccountCloudResourceDTO SetSocialAccountCloudResourceData(CloudPlatformConfiguration configuration, string userId)
        {
            string taskDefinition = $"{Guid.NewGuid()}-task-def";
            string containerName = $"hal-{Guid.NewGuid()}-container";
            string halId = $"{Guid.NewGuid()}";
            SocialAccountCloudResourceDTO userSetup = default;
            try
            {
                userSetup = new()
                {
                    HalId = halId,
                    EcsTaskDefinition = new()
                    {
                        // name of the hal container
                        ContainerName = containerName,
                        Family = taskDefinition,
                        Cpu = configuration.EcsTaskDefinitionConfig.Cpu,
                        ContainerDefinitions = configuration.EcsTaskDefinitionConfig.ContainerDefinitions.Select(c => new ContainerDefinitionDTO
                        {
                            Image = c.Image,
                            Name = taskDefinition,
                            EnviornmentVariables = new()
                            {
                                new KeyValuePair<string, string>(Enum.GetName(DockerEnvironmentVariables.HAL_ID), halId)
                            }
                        }).ToList(),
                        ExecutionRoleArn = configuration.EcsTaskDefinitionConfig.ExecutionRoleArn,
                        TaskRoleArn = configuration.EcsTaskDefinitionConfig.TaskRoleArn,
                        Memory = configuration.EcsTaskDefinitionConfig.Memory,
                        NetworkMode = configuration.EcsTaskDefinitionConfig.NetworkMode,
                        RequiresCompatibilities = configuration.EcsTaskDefinitionConfig.RequiresCompatibilities
                    },
                    CloudMapServiceDiscovery = new()
                    {
                        // name used to discover this service by in the future
                        Name = $"hal-{Guid.NewGuid()}-srv-disc",
                        NamespaceId = configuration.ServiceDiscoveryConfig.NamespaceId,
                        DnsConfig = new()
                        {
                            CloudMapDnsRecords = new()
                            {
                                new()
                                {
                                    TTL = configuration.ServiceDiscoveryConfig.DnsRecordTTL,
                                    Type = configuration.ServiceDiscoveryConfig.DnsRecordType
                                }
                            }
                        }
                    },
                    EcsService = new()
                    {
                        ClusterArn = configuration.EcsServiceConfig.ClusterArn,
                        ServiceName = $"hal-{Guid.NewGuid()}-service",
                        TaskDefinition = taskDefinition,
                        //UserId = userId,
                        AssignPublicIp = configuration.EcsServiceConfig.AssignPublicIp,
                        DesiredCount = configuration.EcsServiceConfig.DesiredCount,
                        SchedulingStrategy = configuration.EcsServiceConfig.SchedulingStrategy,
                        LaunchType = configuration.EcsServiceConfig.LaunchType,
                        Subnets = configuration.EcsServiceConfig.Subnets,
                        SecurityGroups = configuration.EcsServiceConfig.SecurityGroups
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured generating users social account resource class.");
            }

            return userSetup;
        }
        private async Task<ExistingSocialAccountSetupResultDTO> PerformHalsHealthCheckWithPrivateIpAsync(CloudMapServiceDiscoveryConfig serviceDiscoveryConfig, CloudMapDiscoveryService usersCloudDiscoveryService, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResultDTO result = new()
            {
                Succeeded = false
            };

            // Query DNS for fresh ip address for the given service discovery name
            CloudPlatformOperationResult dnsPrivateIpAddressResult = await QueryDNSForTasksPrivateIpAddressAsync(serviceDiscoveryConfig.NamespaceId, usersCloudDiscoveryService.Name, serviceDiscoveryConfig.Name, ct);

            if (dnsPrivateIpAddressResult.Succeeded == false)
            {
                result.Failures = dnsPrivateIpAddressResult.Failures;
                result.IsHalHealthy = false;
                return result;
            }

            // perform healthcheck with fresh private ip address
            HealthCheckRequest halHealthCheckWithPrivateIpRequest = new()
            {
                PrivateIpAddress = (string)dnsPrivateIpAddressResult.Value,
                NamespaceName = serviceDiscoveryConfig.Name,
                RequestUrl = HealthCheckEndpoint
            };

            // perform health check on hal
            HalHealthCheckResponse healthCheckWithPrivateIpResponse = await PerformHalsHealthCheckAsync(halHealthCheckWithPrivateIpRequest, ct);
            if (healthCheckWithPrivateIpResponse.Succeeded == false)
            {
                _logger.LogWarning("Failed to send healthcheck with private ip address to existing resource. Deleting Cloud Resource from this social account.");

                result.Failures.Add(new()
                {
                    Code = Codes.HEALTHCHECK_FAILURE,
                    Detail = "Using private ip address health check failed.",
                    Reason = "Hal's health check failed."
                });
                result.IsHalHealthy = false;
                return result;
            }

            result.IsHalHealthy = true;
            result.Succeeded = true;
            return result;
        }
        private async Task<CloudPlatformOperationResult> QueryDNSForTasksPrivateIpAddressAsync(string namespaceId, string serviceDiscoveryName, string privateNameSpaceName, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            // Query DNS directly and get private ip address for this task and use that to send health check
            result = await GetNamespaceDetailsAsync(namespaceId, ct);
            if (result.Succeeded == false)
            {
                return result;
            }

            string hostedZoneId = (string)result.Value;
            Amazon.Route53.Model.ListResourceRecordSetsResponse listResourceRecordSetsResponse = await ListResourceRecordSetsAsync(hostedZoneId, ct);

            if (listResourceRecordSetsResponse == null || listResourceRecordSetsResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_ERROR,
                    Detail = "Failed to get a list of resource record sets from route 53 in aws",
                    Reason = "Failed to get list resource record sets."
                });
                return result;
            }

            Amazon.Route53.Model.ResourceRecordSet recordSet = listResourceRecordSetsResponse.ResourceRecordSets.Find(r => r.Name == $"{serviceDiscoveryName}.{privateNameSpaceName}.");
            if (recordSet == null)
            {
                _logger.LogError("Route 53 DNS record did not contain an entry for | {serviceDiscoveryName}.{privateNameSpaceName}. |", serviceDiscoveryName, privateNameSpaceName);
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_RESPONSE_MISSING_DATA,
                    Reason = "Failed to locate discovery service",
                    Detail = $"Route 53 DNS record did not contain an entry for {serviceDiscoveryName}"
                });

                return result;
            }

            string privateIpAddressFromDns = string.Empty;
            if (recordSet.ResourceRecords.Count > 1)
            {
                int resourceRecordsCount = recordSet.ResourceRecords.Count;
                _logger.LogWarning("Record set had multiple resource records. Expected to be 1 but found {resourceRecordsCount}. Grabbing first one, this may not be the desired result!", resourceRecordsCount);
            }
            privateIpAddressFromDns = recordSet.ResourceRecords.FirstOrDefault().Value;

            result.Succeeded = true;
            result.Value = privateIpAddressFromDns;
            return result;
        }
        private async Task<CloudPlatformOperationResult> GetNamespaceDetailsAsync(string namespaceId, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            Amazon.ServiceDiscovery.Model.GetNamespaceResponse response = await GetNamespaceResponseAsync(namespaceId, ct);

            if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Get namespace details response was null or the status code was not 200 OK");
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_ERROR,
                    Reason = "An error occured sending request to aws api for namepsace details",
                    Detail = "Failed to get namespace details"
                });
                return result;
            }

            string hostedZoneIp = response.Namespace?.Properties?.DnsProperties?.HostedZoneId;

            if (hostedZoneIp == null)
            {
                _logger.LogError("Failed to find HostedZoneId on the response");
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_RESPONSE_MISSING_DATA,
                    Reason = "Response was missing data",
                    Detail = "Failed to find hosted zone id on the response"
                });
                return result;
            }

            result.Value = hostedZoneIp;
            result.Succeeded = true;
            return result;
        }
        private async Task<Amazon.Route53.Model.ListResourceRecordSetsResponse> ListResourceRecordSetsAsync(string hostedZoneId, CancellationToken ct = default)
        {
            ListRoute53ResourceRecordSetsRequest request = new()
            {
                HostedZoneId = hostedZoneId
            };
            return await _awsRoute53Service.ListResourceRecordSetsAsync(request, ct);
        }
        private async Task<Amazon.ServiceDiscovery.Model.GetNamespaceResponse> GetNamespaceResponseAsync(string namespaceId, CancellationToken ct = default)
        {
            GetCloudMapNamespaceRequest request = new()
            {
                Id = namespaceId
            };

            return await _awsServiceDiscoveryService.GetNamespaceAsync(request, ct);
        }
        private async Task<HalHealthCheckResponse> PerformHalsHealthCheckAsync(HealthCheckRequest healthCheckRequest, CancellationToken ct = default)
        {
            HalHealthCheckResponse result = new()
            {
                Succeeded = false
            };

            HttpResponseMessage response = await _leadslyBotApiService.PerformHealthCheckAsync(healthCheckRequest, ct);
            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.HEALTHCHECK_FAILURE,
                    Detail = "Failed to perform healthcheck on hal",
                    Reason = "An error occured while sending an http request to hal"
                });
                return result;
            }

            try
            {
                string content = await response.Content.ReadAsStringAsync();
                result.Value = JsonConvert.DeserializeObject<HalHealthCheck>(content);
                _logger.LogInformation("Successfully deserialized hal's healthcheck response {content}", content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize hals healthcheck response.");
                result.Failures.Add(new()
                {
                    Code = Codes.OBJECT_MAPPING,
                    Reason = "Deserialization error occured",
                    Detail = "Failed to deserialize hals healthcheck response"
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }
        private async Task<ExistingSocialAccountSetupResultDTO> ValidateEcsServiceForSocialAccountAsync(Amazon.ECS.Model.Service ecsServiceDetails, SocialAccount socialAccount, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResultDTO result = new()
            {
                Succeeded = false
            };

            // check if service is still active
            if (ecsServiceDetails.Status != EcsServiceStatus.ACTIVE)
            {
                // await HandleNonActiveEcsServiceAsync(ecsServiceDetails, socialAccount, ct);
                result.EcsServiceActive = false;
                return result;
            }

            // if ecs service has a different number of running tasks than the desired count something is may be wrong
            if (ecsServiceDetails.RunningCount != ecsServiceDetails.DesiredCount)
            {
                result = await EnsureEcsServiceHasRunningTasksAsync(ecsServiceDetails, ct);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }

            result.Succeeded = true;
            return result;
        }
        private async Task<ExistingSocialAccountSetupResultDTO> EnsureEcsServiceHasRunningTasksAsync(Amazon.ECS.Model.Service ecsServiceDetails, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResultDTO result = new()
            {
                Succeeded = false,
                EcsServiceHasPendingTasks = false,
                EcsTaskRunning = false
            };

            string ecsServiceArn = ecsServiceDetails.ServiceArn;
            _logger.LogWarning("The number of running tasks is different than the desired count. Ecs service arn {ecsServiceArn}", ecsServiceArn);
            int runningTasksCount = ecsServiceDetails.RunningCount;
            int desiredTasksCount = ecsServiceDetails.DesiredCount;
            _logger.LogInformation("Number of desired tasks for this ecs service is ", desiredTasksCount);
            _logger.LogInformation("Number of running tasks for this ecs service is ", runningTasksCount);
            if (ecsServiceDetails.PendingCount != 0)
            {
                result.EcsServiceHasPendingTasks = true;
                int pendingTasksCount = ecsServiceDetails.PendingCount;
                _logger.LogWarning("Ecs service has some pending task.");
                _logger.LogInformation("Number of ecs service pending tasks is {pendingTasksCount}", pendingTasksCount);

                // if ecs service has pending tasks wait up to 2 mins 30 seconds before moving on, if it still fails return failed result and remove the social account and have the user try again
                result = await WaitForPendingTasksToStartAsync(ecsServiceDetails, ct);
                if (result.Succeeded == false)
                {
                    _logger.LogWarning("Ecs service did not successfully start its pending tasks in the allotted time.");
                    result.Failures.Add(new()
                    {
                        Code = Codes.AWS_CLOUD_UNEXPECTED_STATE,
                        Detail = "Ecs service did not resolve pending tasks in the default alotted time",
                        Arn = ecsServiceArn,
                        Reason = "Ecs service still has pending tasks"
                    });
                }
                else
                {
                    // else if everything was successful set pending tasks flag to false
                    result.EcsServiceHasPendingTasks = false;
                }
            }

            if (ecsServiceDetails.RunningCount == 0)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_CLOUD_UNEXPECTED_STATE,
                    Reason = "No running tasks found",
                    Detail = "Ecs service does not have any running tasks"
                });
            }

            // If ecs service has a running task continue with the happy path 
            if (ecsServiceDetails.RunningCount > 0)
            {
                _logger.LogInformation("Ecs service has at least one running task.");
                result.EcsTaskRunning = true;
            }

            result.Succeeded = true;
            return result;
        }
        private async Task<ExistingSocialAccountSetupResultDTO> WaitForPendingTasksToStartAsync(Amazon.ECS.Model.Service ecsServiceDetails, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResultDTO result = new()
            {
                Succeeded = false
            };
            CloudPlatformOperationResult pendingTasksCheck = default;
            Stopwatch mainStopWatch = new Stopwatch();
            Stopwatch stopWatch = new Stopwatch();
            mainStopWatch.Start();
            stopWatch.Start();
            while (mainStopWatch.Elapsed.Seconds >= DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
            {
                // Check elapsed time w/o stopping/resetting the stopwatch                
                if (stopWatch.Elapsed.Seconds >= 15)
                {
                    // At least 15 seconds elapsed, restart stopwatch.
                    stopWatch.Stop();
                    pendingTasksCheck = await CheckIfTasksAreStillPendingAsync(ecsServiceDetails, ct);
                    if (((bool)pendingTasksCheck.Value) == false)
                    {
                        result.Succeeded = true;
                        break;
                    }
                    stopWatch.Start();
                }

                if (mainStopWatch.Elapsed.Seconds == DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
                {
                    result.Failures.Add(new()
                    {
                        Code = Codes.AWS_CLOUD_UNEXPECTED_STATE,
                        Detail = "Expected ecs tasks to be running, but are still pending",
                        Reason = "Ecs tasks are still pending"
                    });
                }
            }

            return result;
        }
        private async Task<CloudPlatformOperationResult> CheckIfTasksAreStillPendingAsync(Amazon.ECS.Model.Service ecsServiceDetails, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            Amazon.ECS.Model.DescribeServicesResponse response = await DescribeServicesAsync(ecsServiceDetails, ct);

            if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                string ecsServiceArn = ecsServiceDetails.ServiceArn;
                _logger.LogInformation("Ecs service arn {ecsServiceArn}", ecsServiceArn);
                _logger.LogWarning("Request to check and see if ecs service tasks were still pending failed.");
                return result;
            }

            if (response.Failures.Count > 0)
            {
                result.Succeeded = false;
                HandleAwsFailrues(response.Failures);
            }

            if (response.Services.Count > 1)
            {
                int servicesCount = response.Services.Count();
                _logger.LogWarning("Aws api returned more than one ecs service. Expected one by got: {servicesCount}", servicesCount);
            }

            var serviceDetails = response.Services.FirstOrDefault();
            result.Succeeded = true;
            result.Value = serviceDetails.PendingCount > 0;
            return result;
        }
        private async Task HandleNonActiveEcsServiceAsync(Amazon.ECS.Model.Service ecsServiceDetails, SocialAccount socialAccount, CancellationToken ct = default)
        {
            //ExistingSocialAccountSetupResult result = new()
            //{
            //    Succeeded = false
            //};

            // if task is no longer active log the ecs service, save it in the database for further investigation and manual removal,
            // return failure from here, but also remove this social account from users list so they can get a new space allocated for themselves
            _logger.LogWarning("User's existing Ecs Service is no longer active. Removing this user's social account from user and saving the ecs service as an orphaned resource. " +
                "This is done with purpose to allow further troubleshooting and so that user can try again but this time a new container will be allocated to them.");

            OrphanedCloudResource orphanedCloudResource = new()
            {
                Arn = ecsServiceDetails.ServiceArn,
                FriendlyName = "Ecs Service",
                Reason = $"Ecs Service Status is {ecsServiceDetails.Status}. Expected {EcsServiceStatus.ACTIVE}",
                ResourceName = ecsServiceDetails.ServiceName,
                UserId = socialAccount.UserId
            };

            await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
        }
        private async Task<DescribeEcsServiceResponse> GetEcsServiceDetailsAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            DescribeEcsServiceResponse result = new()
            {
                Succeeded = false
            };

            // 1 query AWS ECS Api for DescribeServices to ensure that the service is still active and there are no issues with it or its tasks
            Amazon.ECS.Model.DescribeServicesResponse describeServicesResponse = await DescribeServicesAsync(socialAccount, ct);
            if (describeServicesResponse == null || describeServicesResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_ERROR,
                    Detail = "DescribeServices response was null or did not return 200 OK",
                    Reason = "Something went wrong with talking with AWS api"
                });
                return result;
            }
            if (describeServicesResponse.Failures.Count() > 0)
            {
                _logger.LogError("DescribeServices api responded with failures.");
                result.Failures = HandleAwsFailrues(describeServicesResponse.Failures);
                return result;
            }

            // check if service contains more than one response.
            if (describeServicesResponse.Services.Count() > 1)
            {
                _logger.LogWarning("DescribeServices api responded with more than 1 services. Expected to only see one service. Grabbing the first service in the list. This may not be the desired behavior.");
                foreach (var item in describeServicesResponse.Services)
                {
                    string serviceName = item.ServiceName;
                    string serviceArn = item.ServiceArn;

                    _logger.LogWarning("Multiple services return. Service name: {serviceName} | Service arn: {serviceArn}", serviceName, serviceArn);
                }
            }

            Amazon.ECS.Model.Service ecsServiceDetails = describeServicesResponse.Services.FirstOrDefault();
            result.Succeeded = true;
            result.Service = ecsServiceDetails;
            return result;
        }
        private List<Failure> HandleAwsFailrues(List<Amazon.ECS.Model.Failure> failrues)
        {
            List<Failure> failuresDTO = new();
            failuresDTO = failrues.Select(f =>
            {
                string reason = f.Reason;
                string arn = f.Arn;
                _logger.LogError("Reason: ", reason);
                _logger.LogError("Arn: ", arn);
                return new Failure
                {
                    Arn = f.Arn,
                    Reason = f.Reason
                };
            }).ToList();

            return failuresDTO;
        }

        [Obsolete("This is no longer being used")]
        private async Task<Amazon.ECS.Model.DescribeServicesResponse> DescribeServicesAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            DescribeEcsServicesRequest request = new()
            {
                Cluster = socialAccount.SocialAccountCloudResource.EcsService.ClusterArn,
                Services = new()
                {
                    socialAccount.SocialAccountCloudResource.EcsService.ServiceName
                }
            };

            return await _awsElasticContainerService.DescribeServicesAsync(request, ct);
        }

        private async Task<Amazon.ECS.Model.DescribeServicesResponse> DescribeServicesAsync(string serviceName, string clusterArn, CancellationToken ct = default)
        {
            DescribeEcsServicesRequest request = new()
            {
                Cluster = clusterArn,
                Services = new()
                {
                    serviceName
                }
            };

            return await _awsElasticContainerService.DescribeServicesAsync(request, ct);
        }

        [Obsolete("This is no longer in use")]
        private async Task<Amazon.ECS.Model.DescribeServicesResponse> DescribeServicesAsync(Amazon.ECS.Model.Service ecsService, CancellationToken ct = default)
        {
            DescribeEcsServicesRequest request = new()
            {
                Cluster = ecsService.ClusterArn,
                Services = new()
                {
                    ecsService.ServiceName
                }
            };

            return await _awsElasticContainerService.DescribeServicesAsync(request, ct);
        }
        private async Task<Amazon.ECS.Model.ListTasksResponse> ListEcsTasksAsync(ListEcsTasksFilterOptions filterOptions, CancellationToken ct = default)
        {
            ListEcsTasksRequest request = new()
            {
                Cluster = filterOptions.Cluster,
                ContainerInstance = filterOptions.ContainerInstance,
                DesiredStatus = Enum.GetName(EcsDesiredStatus.RUNNING),
                Family = filterOptions.Family,
                LaunchType = Enum.GetName(EcsLaunchType.FARGATE)
            };

            return await _awsElasticContainerService.ListTasksAsync(request, ct);
        }
        private async Task<Amazon.ECS.Model.DescribeTasksResponse> DescribeEcsTasksAsync(string clusterArn, List<string> tasks, CancellationToken ct = default)
        {
            DescribeEcsTasksRequest request = new()
            {
                Cluster = clusterArn,
                Tasks = tasks
            };

            return await _awsElasticContainerService.DescribeTasksAsync(request, ct);
        }

        private async Task<NewSocialAccountSetupResult> SetupNewContainerAsync(SocialAccountCloudResourceDTO userSetup, CancellationToken ct = default)
        {
            NewSocialAccountSetupResult result = new()
            {
                Succeeded = false,
                CreateEcsServiceSucceeded = false,
                CreateServiceDiscoveryServiceSucceeded = false,
                CreateEcsTaskDefinitionSucceeded = false,
                IsHalHealthy = false,
                Value = userSetup
            };

            // Register a new task for the user                  
            Amazon.ECS.Model.RegisterTaskDefinitionResponse registerTaskDefinitionResponse = await RegisterTaskDefinitionAsync(userSetup.EcsTaskDefinition, ct);

            if (registerTaskDefinitionResponse == null || registerTaskDefinitionResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_ERROR,
                    Detail = "Response was null or did not equal expected OK 200",
                    Reason = "Failed to register new task definition"
                });
                return result;
            }
            _logger.LogInformation("Successfully registered new task definition");
            result.CreateEcsTaskDefinitionSucceeded = true;
            userSetup.EcsTaskDefinition = UpdateEcsTaskDefinitionValues(registerTaskDefinitionResponse, userSetup.EcsTaskDefinition);

            // Create new AWS Cloud Map Service Discovery Service for this container
            Amazon.ServiceDiscovery.Model.CreateServiceResponse createDiscoveryServiceResponse = await CreateServiceDiscoveryServiceAsync(userSetup.CloudMapServiceDiscovery, ct);

            if (createDiscoveryServiceResponse == null || createDiscoveryServiceResponse.Service == null || createDiscoveryServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_ERROR,
                    Detail = "Response was null or did not equal expected OK 200",
                    Reason = "Failed to create new service discovery service"
                });
                return result;
            }
            _logger.LogInformation("Successfully create new service discovery service in aws Cloud Map");
            result.CreateServiceDiscoveryServiceSucceeded = true;
            // update userSetup with the service discovery id. In case something fails, this will id will be used to clean up aws resources
            userSetup.CloudMapServiceDiscovery = UpdateCloudMapServiceDiscoveryValues(createDiscoveryServiceResponse, userSetup.CloudMapServiceDiscovery);

            // link users ecs service with the created service discovery service
            userSetup.EcsService.Registries = new()
            {
                new()
                {
                    RegistryArn = createDiscoveryServiceResponse.Service.Arn
                }
            };

            // Create new Ecs Service Task with the task definition and assign it to the created service discovery service
            Amazon.ECS.Model.CreateServiceResponse createEcsServiceResponse = await CreateEcsServiceAsync(userSetup.EcsService, ct);

            if (createEcsServiceResponse == null || createEcsServiceResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.AWS_API_ERROR,
                    Detail = "Response was null or did not equal expected OK 200",
                    Reason = "Failed to create new ecs service"
                });
                return result;
            }
            _logger.LogInformation("Successfully create new ecs service");
            result.CreateEcsServiceSucceeded = true;
            userSetup.EcsService = UpdateEcsServiceValues(createEcsServiceResponse, userSetup.EcsService);

            // add the upated userSetup here
            result.Value = userSetup;

            CloudPlatformOperationResult ensureServiceTasksAreRunningResult = await EnsureEcsServiceTasksAreRunningAsync(userSetup, ct);
            if (ensureServiceTasksAreRunningResult.Succeeded == false)
            {
                result.Failures = ensureServiceTasksAreRunningResult.Failures;
                return result;
            }

            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            // perform health check on hal
            HalHealthCheckResponse healthCheckResponse = await RunHalsHealthCheckAsync(userSetup.CloudMapServiceDiscovery.Name, configuration, ct);
            if (healthCheckResponse.Succeeded == false)
            {
                _logger.LogInformation("Running initial hals health check did not succeeded");
                result.Failures = healthCheckResponse.Failures;
                result.IsHalHealthy = false;
                return result;
            }

            result.IsHalHealthy = true;
            result.Succeeded = true;
            return result;
        }
        private EcsTaskDefinitionDTO UpdateEcsTaskDefinitionValues(Amazon.ECS.Model.RegisterTaskDefinitionResponse registerTaskDefinitionResponse, EcsTaskDefinitionDTO taskDefinitionDTO)
        {
            taskDefinitionDTO.TaskDefinitionArn = registerTaskDefinitionResponse.TaskDefinition.TaskDefinitionArn;
            return taskDefinitionDTO;
        }
        private CloudMapDiscoveryServiceDTO UpdateCloudMapServiceDiscoveryValues(Amazon.ServiceDiscovery.Model.CreateServiceResponse createServiceDiscoveryService, CloudMapDiscoveryServiceDTO cloudMapDiscoveryDTO)
        {
            // response
            cloudMapDiscoveryDTO.ServiceDiscoveryId = createServiceDiscoveryService.Service?.Id;
            cloudMapDiscoveryDTO.Arn = createServiceDiscoveryService.Service?.Arn;
            cloudMapDiscoveryDTO.CreateDate = createServiceDiscoveryService.Service?.CreateDate;
            cloudMapDiscoveryDTO.Description = createServiceDiscoveryService.Service?.Description;
            cloudMapDiscoveryDTO.CreateRequestId = createServiceDiscoveryService.Service?.CreatorRequestId;
            return cloudMapDiscoveryDTO;
        }
        private EcsServiceDTO UpdateEcsServiceValues(Amazon.ECS.Model.CreateServiceResponse createEcsServiceResponse, EcsServiceDTO ecsServiceDTO)
        {
            // values from the response
            ecsServiceDTO.ServiceArn = createEcsServiceResponse.Service?.ServiceArn;
            ecsServiceDTO.CreatedAt = createEcsServiceResponse.Service?.CreatedAt;
            ecsServiceDTO.CreatedBy = createEcsServiceResponse.Service?.CreatedBy;

            // return updatedEcsService;
            return ecsServiceDTO;
        }
        [Obsolete("This is no longer in use")]
        private async Task<CloudPlatformOperationResult> EnsureEcsServiceTasksAreRunningAsync(SocialAccountCloudResourceDTO userSetup, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };
            Stopwatch mainStopWatch = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            mainStopWatch.Start();
            _logger.LogInformation("Ensuring ecs service tasks are running. Waiting 20 seconds before checking.");
            while (mainStopWatch.Elapsed.TotalSeconds <= DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
            {
                // Check elapsed time w/o stopping/resetting the stopwatch                
                if (stopwatch.Elapsed.TotalSeconds >= 20)
                {
                    double timeout = (DefaultTimeToWaitForEcsServicePendingTasks_InSeconds - mainStopWatch.Elapsed.TotalSeconds);
                    _logger.LogInformation("Checking if ecs service tasks are running... Times out in: {timeout}", timeout);
                    // At least 5 seconds elapsed, restart stopwatch.
                    stopwatch.Stop();
                    CloudPlatformOperationResult checkTaskStatusResult = await AreEcsServiceTasksRunningAsync(userSetup, ct);
                    if (checkTaskStatusResult.Succeeded)
                    {
                        if (((bool)checkTaskStatusResult.Value) == true)
                        {
                            _logger.LogInformation("Successfully verified ecs service tasks are running");
                            result.Succeeded = true;
                            mainStopWatch.Stop();
                            stopwatch.Stop();
                            break;
                        }
                        else
                        {
                            _logger.LogInformation("Ecs service tasks are not running yet. Checking again in 20 seconds...");
                        }
                    }
                    else
                    {
                        break;
                    }
                    stopwatch.Restart();
                }

                if (mainStopWatch.Elapsed.TotalSeconds == DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
                {
                    _logger.LogWarning("Failed to find out if ecs service tasks are running in the alotted time.");
                    result.Failures.Add(new()
                    {
                        Code = Codes.AWS_CLOUD_UNEXPECTED_STATE,
                        Reason = "Ecs service tasks are still pending",
                        Detail = "Ecs tasks for the ecs service are still pending, but should be running."
                    });
                }
            }

            return result;
        }
        [Obsolete("This is no longer used")]
        private async Task<CloudPlatformOperationResult> AreEcsServiceTasksRunningAsync(SocialAccountCloudResourceDTO userSetup, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            Amazon.ECS.Model.DescribeServicesResponse response = await DescribeServicesAsync(userSetup.EcsService, ct);

            if (response == null || response.Services == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Value = false;
                result.Failures.Add(new()
                {
                    Arn = userSetup.EcsService.ServiceName,
                    Code = Codes.AWS_API_ERROR,
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
            result.Value = response.Services.First().PendingCount == 0 && response.Services.First().RunningCount > 0;
            return result;
        }
        public async Task<bool> EnsureEcsServiceTasksAreRunningAsync(string ecsServiceName, string clusterArn, CancellationToken ct = default)
        {
            Stopwatch mainStopWatch = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            mainStopWatch.Start();
            _logger.LogInformation("Ensuring ecs service tasks are running. Waiting 20 seconds before checking.");
            while (mainStopWatch.Elapsed.TotalSeconds <= DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
            {
                // Check elapsed time w/o stopping/resetting the stopwatch                
                if (stopwatch.Elapsed.TotalSeconds >= 20)
                {
                    double timeout = (DefaultTimeToWaitForEcsServicePendingTasks_InSeconds - mainStopWatch.Elapsed.TotalSeconds);
                    _logger.LogInformation("Checking if ecs service tasks are running... Times out in: {timeout}", timeout);
                    // At least 5 seconds elapsed, restart stopwatch.
                    stopwatch.Stop();
                    CloudPlatformOperationResult checkTaskStatusResult = await AreEcsServiceTasksRunningAsync(ecsServiceName, clusterArn, ct);
                    if (checkTaskStatusResult.Succeeded)
                    {
                        if (((bool)checkTaskStatusResult.Value) == true)
                        {
                            _logger.LogInformation("Successfully verified ecs service tasks are running");
                            mainStopWatch.Stop();
                            stopwatch.Stop();
                            return true;
                        }
                        else
                        {
                            _logger.LogInformation("Ecs service tasks are not running yet. Checking again in 20 seconds...");
                        }
                    }
                    else
                    {
                        return false;
                    }
                    stopwatch.Restart();
                }

                if (mainStopWatch.Elapsed.TotalSeconds == DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
                {
                    _logger.LogWarning("Failed to find out if ecs service tasks are running in the alotted time.");
                    return false;
                }
            }

            return true;
        }
        private async Task<CloudPlatformOperationResult> AreEcsServiceTasksRunningAsync(string ecsServiceName, string clusternArn, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            Amazon.ECS.Model.DescribeServicesResponse response = await DescribeServicesAsync(ecsServiceName, clusternArn, ct);

            if (response == null || response.Services == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                result.Value = false;
                result.Failures.Add(new()
                {
                    Arn = ecsServiceName,
                    Code = Codes.AWS_API_ERROR,
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
            result.Value = response.Services.First().PendingCount == 0 && response.Services.First().RunningCount > 0;
            return result;
        }


        private async Task DeleteServiceDiscoveryServiceAsync(CloudMapDiscoveryServiceDTO serviceDiscovery, string userId, CancellationToken ct = default)
        {
            DeleteServiceDiscoveryServiceRequest request = new()
            {
                Id = serviceDiscovery.ServiceDiscoveryId
            };

            // service discovery service cannot have any instances assigned to it prior to deletion, if we have just deleted ecs service
            // then it might take couple seconds reflect on aws side
            Amazon.ServiceDiscovery.Model.DeleteServiceResponse response = default;
            try
            {
                response = await _awsServiceDiscoveryService.DeleteServiceAsync(request, ct);
            }
            catch (Amazon.ServiceDiscovery.Model.ResourceInUseException ex)
            {
                _logger.LogWarning(ex, "Aws resource in use exception occured. Attempting to delete again");
                response = await DeleteServiceDiscoveryServiceRetryAsync(request, ct);
            }

            if (response == null)
            {
                string serviceDiscoveryArn = serviceDiscovery.Arn;
                _logger.LogWarning("Delete operation for service discovery service failed. Service discovery arn: {serviceDiscoveryArn}", serviceDiscoveryArn);

                OrphanedCloudResource orphanedResource = new()
                {
                    FriendlyName = "Service Discovery Service",
                    ResourceId = serviceDiscovery.ServiceDiscoveryId,
                    Arn = serviceDiscovery.Arn,
                    UserId = userId
                };
                _logger.LogInformation("Adding service discovery service to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedResource, ct);
            }

        }
        private async Task<Amazon.ServiceDiscovery.Model.DeleteServiceResponse> DeleteServiceDiscoveryServiceRetryAsync(DeleteServiceDiscoveryServiceRequest request, CancellationToken ct = default)
        {
            Amazon.ServiceDiscovery.Model.DeleteServiceResponse response = default;
            Stopwatch mainStopwatch = new Stopwatch();
            Stopwatch intervalStopWatch = new Stopwatch();
            mainStopwatch.Start();
            intervalStopWatch.Start();
            while (mainStopwatch.Elapsed.TotalSeconds <= DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
            {
                if (intervalStopWatch.Elapsed.TotalSeconds > 15)
                {
                    intervalStopWatch.Stop();
                    try
                    {
                        response = await _awsServiceDiscoveryService.DeleteServiceAsync(request, ct);

                        mainStopwatch.Stop();
                        intervalStopWatch.Stop();
                        _logger.LogInformation("Successfully deleted Service Discovery Service after retries.");
                        break;
                    }
                    catch (Amazon.ServiceDiscovery.Model.ResourceInUseException ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        intervalStopWatch.Restart();
                    }
                }
            }
            return response;
        }
        private async Task DeleteServiceDiscoveryServiceAsync(CloudMapDiscoveryService serviceDiscovery, string userId, CancellationToken ct = default)
        {
            DeleteServiceDiscoveryServiceRequest request = new()
            {
                Id = serviceDiscovery.ServiceDiscoveryId
            };

            // service discovery service cannot have any instances assigned to it prior to deletion, if we have just deleted ecs service
            // then it might take couple seconds reflect on aws side
            Amazon.ServiceDiscovery.Model.DeleteServiceResponse response = default;
            try
            {
                response = await _awsServiceDiscoveryService.DeleteServiceAsync(request, ct);
            }
            catch (Amazon.ServiceDiscovery.Model.ResourceInUseException ex)
            {
                _logger.LogWarning(ex, "Aws resource in use exception occured. Attempting to delete again");
                response = await DeleteServiceDiscoveryServiceRetryAsync(request, ct);
            }

            if (response == null)
            {
                string serviceDiscoveryArn = serviceDiscovery.Arn;
                _logger.LogWarning("Delete operation for service discovery service failed. Service discovery arn: {serviceDiscoveryArn}", serviceDiscoveryArn);

                OrphanedCloudResource orphanedResource = new()
                {
                    FriendlyName = "Service Discovery Service",
                    ResourceId = serviceDiscovery.ServiceDiscoveryId,
                    Arn = serviceDiscovery.Arn,
                    UserId = userId
                };
                _logger.LogInformation("Adding service discovery service to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedResource, ct);
            }
        }
        private async Task DeleteEcsServiceAsync(EcsServiceDTO EcsServiceDto, string userId, CancellationToken ct = default)
        {
            DeleteEcsServiceRequest request = new()
            {
                Cluster = EcsServiceDto.ClusterArn,
                Force = true,
                Service = EcsServiceDto.ServiceName
            };

            Amazon.ECS.Model.DeleteServiceResponse response = await _awsElasticContainerService.DeleteServiceAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Delete operation for ecs service failed.");

                OrphanedCloudResource orphanedCloudResource = new()
                {
                    UserId = userId,
                    FriendlyName = "Ecs Service",
                    ResourceId = EcsServiceDto.ServiceName
                };
                _logger.LogInformation("Adding ecs service to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
            }
        }
        private async Task DeleteEcsServiceAsync(EcsService EcsService, string userId, CancellationToken ct = default)
        {
            DeleteEcsServiceRequest request = new()
            {
                Cluster = EcsService.ClusterArn,
                Force = true,
                Service = EcsService.ServiceName
            };

            Amazon.ECS.Model.DeleteServiceResponse response = await _awsElasticContainerService.DeleteServiceAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Delete operation for ecs service failed.");

                OrphanedCloudResource orphanedCloudResource = new()
                {
                    UserId = userId,
                    FriendlyName = "Ecs Service",
                    ResourceId = EcsService.ServiceName
                };
                _logger.LogInformation("Adding ecs service to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
            }
        }
        private async Task DeregisterTaskDefinitionAsync(EcsTaskDefinitionDTO taskDefinition, string userId, CancellationToken ct = default)
        {
            DeregisterEcsTaskDefinitionRequest request = new()
            {
                TaskDefinition = taskDefinition.Family
            };

            Amazon.ECS.Model.DeregisterTaskDefinitionResponse response = await _awsElasticContainerService.DeregisterTaskDefinitionAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Deregister operation for ecs task definition failed.");
                OrphanedCloudResource orphanedCloudResource = new()
                {
                    UserId = userId,
                    FriendlyName = "Task definition",
                    ResourceId = taskDefinition.Family
                };

                _logger.LogInformation("Adding task definition to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
            }
        }
        private async Task DeregisterTaskDefinitionAsync(EcsTaskDefinition taskDefinition, string userId, CancellationToken ct = default)
        {
            DeregisterEcsTaskDefinitionRequest request = new()
            {
                TaskDefinition = taskDefinition.Family
            };

            Amazon.ECS.Model.DeregisterTaskDefinitionResponse response = await _awsElasticContainerService.DeregisterTaskDefinitionAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Deregister operation for ecs task definition failed.");
                OrphanedCloudResource orphanedCloudResource = new()
                {
                    UserId = userId,
                    FriendlyName = "Task definition",
                    ResourceId = taskDefinition.Family
                };

                _logger.LogInformation("Adding task definition to orphaned cloud resources table for manual clean up.");
                await _orphanedCloudResourcesRepository.AddOrphanedCloudResourceAsync(orphanedCloudResource, ct);
            }
        }
        private async Task<Amazon.ECS.Model.CreateServiceResponse> CreateEcsServiceAsync(EcsServiceDTO EcsService, CancellationToken ct = default)
        {
            CreateEcsServiceRequest createEcsServiceRequest = new()
            {
                AssignPublicIp = EcsService.AssignPublicIp,
                Cluster = EcsService.ClusterArn,
                DesiredCount = EcsService.DesiredCount,
                LaunchType = EcsService.LaunchType,
                SchedulingStrategy = EcsService.SchedulingStrategy,
                SecurityGroups = EcsService.SecurityGroups,
                Subnets = EcsService.Subnets,
                TaskDefinition = EcsService.TaskDefinition,
                ServiceName = EcsService.ServiceName,
                EcsServiceRegistries = EcsService.Registries.Select(r => new Leadsly.Application.Model.Aws.ElasticContainerService.EcsServiceRegistry
                {
                    RegistryArn = r.RegistryArn
                }).ToList()
            };

            return await _awsElasticContainerService.CreateServiceAsync(createEcsServiceRequest, ct);
        }
        private async Task<Amazon.ServiceDiscovery.Model.CreateServiceResponse> CreateServiceDiscoveryServiceAsync(CloudMapDiscoveryServiceDTO cloudMapServiceDiscovery, CancellationToken ct = default)
        {
            CreateServiceDiscoveryServiceRequest createServiceDiscoveryServiceRequest = new()
            {
                Name = cloudMapServiceDiscovery.Name,
                NamespaceId = cloudMapServiceDiscovery.NamespaceId,
                DnsConfig = new()
                {
                    CloudMapDnsRecords = cloudMapServiceDiscovery.DnsConfig.CloudMapDnsRecords.Select(rec => new CloudMapDnsRecord()
                    {
                        TTL = rec.TTL,
                        Type = rec.Type
                    }).ToList()
                }
            };

            return await _awsServiceDiscoveryService.CreateServiceAsync(createServiceDiscoveryServiceRequest, ct);
        }
        private async Task<Amazon.ECS.Model.RegisterTaskDefinitionResponse> RegisterTaskDefinitionAsync(EcsTaskDefinitionDTO taskDefinition, CancellationToken ct = default)
        {
            RegisterEcsTaskDefinitionRequest registerEcsTaskDefinitionRequest = new()
            {
                Cpu = taskDefinition.Cpu,
                Family = taskDefinition.Family,
                EcsContainerDefinitions = taskDefinition.ContainerDefinitions.Select(c => new EcsContainerDefinition
                {
                    Image = c.Image,
                    Name = taskDefinition.ContainerName,
                    EnviornmentVariables = c.EnviornmentVariables
                }).ToList(),
                ExecutionRoleArn = taskDefinition.ExecutionRoleArn,
                TaskRoleArn = taskDefinition.TaskRoleArn,
                Memory = taskDefinition.Memory,
                NetworkMode = taskDefinition.NetworkMode,
                RequiresCompatibilities = taskDefinition.RequiresCompatibilities,
            };

            return await _awsElasticContainerService.RegisterTaskDefinitionAsync(registerEcsTaskDefinitionRequest, ct);
        }
        private async Task<Amazon.ECS.Model.DescribeServicesResponse> DescribeServicesAsync(EcsServiceDTO userService, CancellationToken ct = default)
        {
            List<string> services = new() { userService.ServiceName };
            string cluster = userService.ClusterArn;

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

        public async Task<VirtualAssistant> GetVirtualAssistantAsync(string userId, CancellationToken ct = default)
        {
            IList<VirtualAssistant> virtualAssistants = await _cloudPlatformRepository.GetAllVirtualAssistantByUserIdAsync(userId, ct);

            // for now we only expect users to have only one assistant.
            return virtualAssistants.FirstOrDefault();
        }

        public async Task<VirtualAssistant> CreateVirtualAssistantAsync(
            EcsTaskDefinition newEcsTaskDefinition,
            EcsService newEcsService,
            CloudMapDiscoveryService newCloudMapService,
            string halId,
            string userId,
            string timezoneId,
            CancellationToken ct = default)
        {
            HalUnit newHalUnit = new()
            {
                HalId = halId,
                TimeZoneId = timezoneId,
                ApplicationUserId = userId
            };

            newCloudMapService.EcsService = newEcsService;
            newEcsService.CloudMapDiscoveryService = newCloudMapService;

            VirtualAssistant virtualAssistant = new VirtualAssistant
            {
                EcsTaskDefinition = newEcsTaskDefinition,
                EcsService = newEcsService,
                CloudMapDiscoveryService = newCloudMapService,
                ApplicationUserId = userId,
                HalUnit = newHalUnit,
                HalId = halId,
            };

            return await _cloudPlatformRepository.CreateVirtualAssistantAsync(virtualAssistant, ct);
        }
    }
}
