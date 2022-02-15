using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Models;
using Leadsly.Models.Aws.ElasticContainerService;
using Leadsly.Models.Aws.Route53;
using Leadsly.Models.Aws.ServiceDiscovery;
using Leadsly.Models.Entities;
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
            ISocialAccountRepository socialAccountRepository,
            ILeadslyBotApiService leadslyBotApiService,
            IOrphanedCloudResourcesRepository orphanedCloudResourcesRepository,
            ILogger<CloudPlatformProvider> logger)
        {
            _awsElasticContainerService = awsElasticContainerService;
            _awsServiceDiscoveryService = awsServiceDiscoveryService;
            _cloudPlatformRepository = cloudPlatformRepository;
            _orphanedCloudResourcesRepository = orphanedCloudResourcesRepository;
            _socialAccountRepository = socialAccountRepository;
            _awsRoute53Service = awsRoute53Service;
            _leadslyBotApiService = leadslyBotApiService;
            _logger = logger;
        }

        private readonly ILeadslyBotApiService _leadslyBotApiService;
        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly IAwsRoute53Service _awsRoute53Service;
        private readonly IAwsServiceDiscoveryService _awsServiceDiscoveryService;
        private readonly IOrphanedCloudResourcesRepository _orphanedCloudResourcesRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILogger<CloudPlatformProvider> _logger;
        private readonly int DefaultTimeToWaitForEcsServicePendingTasks_InSeconds = 150;        

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
                    Detail = "Cannot get ecs service, ecs task definition or service discovery required configuration",
                    Reason = "Failed to retrieve configuration details"
                });
                return result;
            }

            string taskDefinition = $"{Guid.NewGuid()}";
            SocialAccountCloudResourceDTO userSetup = new()
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
                    TaskDefinition = taskDefinition,
                    UserId = userId
                }
            };

            return await SetupNewContainerAsync(userSetup, configuration, ct);
        }

        public async Task<ExistingSocialAccountSetupResult> ConnectToExistingCloudResourceAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResult result = new()
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

            CloudPlatformConfiguration configuration = _cloudPlatformRepository.GetCloudPlatformConfiguration();
            HalRequest halHealthCheckRequest = new()
            {
                DiscoveryServiceName = socialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService.Name,
                NamespaceName = configuration.ServiceDiscoveryConfig.Name,
                RequestUrl = "healthcheck"
            };
            // perform health check on hal
            HalHealthCheckResponse healthCheckResponse = await PerformHalsHealthCheckAsync(halHealthCheckRequest, ct);
            if(healthCheckResponse.Succeeded == false)
            {
                // if hals check fails query the dns record for private ip address of this resource and try again, its possible that the dns record has stale ip address
                CloudPlatformOperationResult namespaceDetailsResult = await GetNamespaceDetailsAsync(configuration.ServiceDiscoveryConfig.NamespaceId, ct);
                if(namespaceDetailsResult.Succeeded == false)
                {
                    result.Failures = namespaceDetailsResult.Failures;
                    return result;
                }

                string hostedZoneId = (string)namespaceDetailsResult.Value;
                Amazon.Route53.Model.ListResourceRecordSetsResponse listResourceRecordSetsResponse = await ListResourceRecordSetsAsync(hostedZoneId, ct);

                if(listResourceRecordSetsResponse == null || listResourceRecordSetsResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    result.Failures.Add(new()
                    {
                        Detail = "Failed to get a list of resource record sets from route 53 in aws",
                        Reason = "Failed to get list resource record sets."
                    });
                    return result;
                }

                Amazon.Route53.Model.ResourceRecordSet recordSet = listResourceRecordSetsResponse.ResourceRecordSets.Find(r => r.Name == socialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService.Name);
                if(recordSet == null)
                {
                    string serviceDiscoveryName = socialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService.Name;
                    _logger.LogError("Route 53 DNS record did not contain an entry for {serviceDiscoveryName}", serviceDiscoveryName);
                    result.Failures.Add(new()
                    {
                        Reason = "Failed to locate discovery service",
                        Detail = $"Route 53 DNS record did not contain an entry for {socialAccount.SocialAccountCloudResource.CloudMapServiceDiscoveryService.Name}"
                    });

                    return result;
                }

                string privateIpAddressFromDns = string.Empty;
                if(recordSet.ResourceRecords.Count > 1)
                {
                    int resourceRecordsCount = recordSet.ResourceRecords.Count;
                    _logger.LogWarning("Record set had multiple resource records. Expected to be 1 but found {resourceRecordsCount}. Grabbing first one, this may not be the desired result!", resourceRecordsCount);
                    privateIpAddressFromDns = recordSet.ResourceRecords.FirstOrDefault().Value;
                }

            }

            // in the very slim chance that this ip address is pointing to a recycled ip address that is running a different version of hal lets check the container names
            if(healthCheckResponse.Value.HalsUniqueName != socialAccount.SocialAccountCloudResource.ContainerName)
            {
                _logger.LogError("Rare edge case occured. Hal's health check successfully responded but the name of hal running in the container is different then what user has registered to their name. Requring DNS to get fresh private ip address.");
                CloudPlatformOperationResult namespaceDetailsResult = await GetNamespaceDetailsAsync(configuration.ServiceDiscoveryConfig.NamespaceId, ct);
                if (namespaceDetailsResult.Succeeded == false)
                {
                    result.Failures = namespaceDetailsResult.Failures;
                    return result;
                }
            }

            return result;
        }

        public async Task RollbackCloudResourcesAsync(NewSocialAccountSetupResult setupToRollback, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Aws resource cleanup started.");
            SocialAccountCloudResourceDTO rollbackDetails = setupToRollback.Value;
            // first start removing service discovery service
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

            // then ecs service
            if (setupToRollback.CreateEcsServiceSucceeded)
            {
                _logger.LogInformation("Starting to delete ecs service.");
                await DeleteEcsServiceAsync(rollbackDetails.EcsService, userId, ct);
                _logger.LogInformation("Finished deleting ecs service.");
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

        private async Task<CloudPlatformOperationResult> GetNamespaceDetailsAsync(string namespaceId, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };

            Amazon.ServiceDiscovery.Model.GetNamespaceResponse response = await GetNamespaceResponseAsync(namespaceId, ct);

            if(response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Get namespace details response was null or the status code was not 200 OK");
                result.Failures.Add(new()
                {
                    Reason = "An error occured sending request to aws api for namepsace details",
                    Detail = "Failed to get namespace details"
                });
                return result;
            }

            string hostedZoneIp = response.Namespace?.Properties?.DnsProperties?.HostedZoneId;

            if(hostedZoneIp == null)
            {
                _logger.LogError("Failed to find HostedZoneId on the response");
                result.Failures.Add(new()
                {
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
        private async Task<HalHealthCheckResponse> PerformHalsHealthCheckAsync(HalRequest healthCheckRequest, CancellationToken ct = default)
        {
            HalHealthCheckResponse result = new()
            {
                Succeeded = false
            };

            HttpResponseMessage response = await _leadslyBotApiService.PerformHealthCheckAsync(healthCheckRequest, ct);
            if (response.IsSuccessStatusCode == false)
            {
                result.Failures.Add(new()
                {
                    Detail = $"Failed to perform healthcheck on hal",
                    Reason = "An error occured while sending an http request to hal"
                });
                return result;
            }

            try
            {
                result.Value = JsonConvert.DeserializeObject<HalHealthCheck>(await response.Content.ReadAsStringAsync());                 
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize hals healthcheck response.");                
                result.Failures.Add(new() 
                {
                    Reason = "Deserialization error occured",
                    Detail = "Failed to deserialize hals healthcheck response"
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }
        private async Task<ExistingSocialAccountSetupResult> ValidateEcsServiceForSocialAccountAsync(Amazon.ECS.Model.Service ecsServiceDetails, SocialAccount socialAccount, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResult result = new()
            {
                Succeeded = false
            };

            // check if service is still active
            if (ecsServiceDetails.Status != EcsServiceStatus.ACTIVE)
            {
                result = await HandleNonActiveEcsServiceAsync(ecsServiceDetails, socialAccount, ct);
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

        private async Task<ExistingSocialAccountSetupResult> EnsureEcsServiceHasRunningTasksAsync(Amazon.ECS.Model.Service ecsServiceDetails, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResult result = new()
            {
                Succeeded = false
            };

            string ecsServiceArn = ecsServiceDetails.ServiceArn;
            _logger.LogWarning("The number of running tasks is different than the desired count. Ecs service arn {ecsServiceArn}", ecsServiceArn);
            int runningTasksCount = ecsServiceDetails.RunningCount;
            int desiredTasksCount = ecsServiceDetails.DesiredCount;
            _logger.LogInformation("Number of desired tasks for this ecs service is ", desiredTasksCount);
            _logger.LogInformation("Number of running tasks for this ecs service is ", runningTasksCount);
            if (ecsServiceDetails.PendingCount != 0)
            {
                int pendingTasksCount = ecsServiceDetails.PendingCount;
                _logger.LogWarning("Ecs service has some pending task.");
                _logger.LogInformation("Number of ecs service pending tasks is {pendingTasksCount}", pendingTasksCount);

                // if ecs service has pending tasks wait up to 2 mins 30 seconds before moving on, if it still fails return failed result and remove the social account and have the user try again
                result = await WaitForPendingTasksToStartAsync(ecsServiceDetails, ct);
                if(result.Succeeded == false)
                {
                    result.Failures.Add(new()
                    {
                        Detail = "Ecs service did not resolve pending tasks in the default alotted time",
                        Arn = ecsServiceArn,
                        Reason = "Ecs service still has pending tasks"
                    });
                    return result;
                }
            }

            result.Succeeded = true;
            return result;
        }

        private async Task<ExistingSocialAccountSetupResult> WaitForPendingTasksToStartAsync(Amazon.ECS.Model.Service ecsServiceDetails, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResult result = new()
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

            if(response == null || response.HttpStatusCode != HttpStatusCode.OK)
            {
                string ecsServiceArn = ecsServiceDetails.ServiceArn;
                _logger.LogInformation("Ecs service arn {ecsServiceArn}", ecsServiceArn);
                _logger.LogWarning("Request to check and see if ecs service tasks were still pending failed.");
                return result;
            }

            if(response.Failures.Count > 0)
            {
                result.Succeeded = false;
                HandleAwsFailrues(response.Failures);
            }

            if(response.Services.Count > 1)
            {
                int servicesCount = response.Services.Count();
                _logger.LogWarning("Aws api returned more than one ecs service. Expected one by got: {servicesCount}", servicesCount);
            }

            var serviceDetails = response.Services.FirstOrDefault();
            result.Succeeded = true;
            result.Value = serviceDetails.PendingCount > 0;
            return result;
        }

        private async Task<ExistingSocialAccountSetupResult> HandleNonActiveEcsServiceAsync(Amazon.ECS.Model.Service ecsServiceDetails, SocialAccount socialAccount, CancellationToken ct = default)
        {
            ExistingSocialAccountSetupResult result = new()
            {
                Succeeded = false
            };

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

            bool deleteOperationResult = await _socialAccountRepository.RemoveSocialAccountAsync(socialAccount.Id, ct);

            if (deleteOperationResult == false)
            {
                _logger.LogWarning("Failed to delete user's social account. Please contact support for further assistance. This removal has to be done manually.");
                result.Failures.Add(new()
                {
                    Reason = "Failed to remove social account",
                    Detail = "Error occured when remvoing social account from user. Please contact support for assistance."
                });
            }

            return result;
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

        private List<FailureDTO> HandleAwsFailrues(List<Amazon.ECS.Model.Failure> failrues)
        {
            List<FailureDTO> failuresDTO = new();
            failuresDTO = failrues.Select(f =>
            {
                string reason = f.Reason;
                string detail = f.Detail;
                string arn = f.Arn;
                _logger.LogError("Reason: ", reason);
                _logger.LogError("Detail: ", detail);
                _logger.LogError("Arn: ", arn);
                return new FailureDTO
                {
                    Arn = f.Arn,
                    Detail = f.Detail,
                    Reason = f.Reason
                };
            }).ToList();

            return failuresDTO;
        }

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

        private async Task<NewSocialAccountSetupResult> SetupNewContainerAsync(SocialAccountCloudResourceDTO userSetup, CloudPlatformConfiguration configuration, CancellationToken ct = default)
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
            userSetup.CloudMapServiceDiscovery.Arn = createDiscoveryServiceResponse.Service?.Arn;
            userSetup.CloudMapServiceDiscovery.CreateDate = createDiscoveryServiceResponse.Service?.CreateDate;
            userSetup.CloudMapServiceDiscovery.NamepaceId = createDiscoveryServiceResponse.Service?.NamespaceId;
            userSetup.CloudMapServiceDiscovery.Description = createDiscoveryServiceResponse.Service?.Description;
            userSetup.CloudMapServiceDiscovery.CreateRequestId = createDiscoveryServiceResponse.Service?.Description;

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
            userSetup.EcsService.ServiceArn = createEcsServiceResponse.Service?.ServiceArn;
            userSetup.EcsService.CreatedAt = createEcsServiceResponse.Service?.CreatedAt;
            userSetup.EcsService.CreatedBy = createEcsServiceResponse.Service?.CreatedBy;
            
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

        private async Task<CloudPlatformOperationResult> EnsureEcsServiceTasksAreRunningAsync(SocialAccountCloudResourceDTO userSetup, CloudPlatformConfiguration configuration, CancellationToken ct = default)
        {
            CloudPlatformOperationResult result = new()
            {
                Succeeded = false
            };
            Stopwatch mainStopWatch = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (mainStopWatch.Elapsed.TotalSeconds >= DefaultTimeToWaitForEcsServicePendingTasks_InSeconds)
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

        private async Task<CloudPlatformOperationResult> AreEcsServiceTasksRunningAsync(SocialAccountCloudResourceDTO userSetup, CloudPlatformConfiguration configuration, CancellationToken ct = default)
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

        private async Task DeleteServiceDiscoveryServiceAsync(CloudMapServiceDiscoveryServiceDTO serviceDiscovery, string userId, CancellationToken ct = default)
        {
            DeleteServiceDiscoveryServiceRequest request = new()
            {
                Id = serviceDiscovery.Id
            };

            Amazon.ServiceDiscovery.Model.DeleteServiceResponse response = await _awsServiceDiscoveryService.DeleteServiceAsync(request, ct);
            if (response == null)
            {
                _logger.LogWarning("Delete operation for service discovery service failed.");

                OrphanedCloudResource orphanedResource = new()
                {
                    FriendlyName = "Service Discovery Service",
                    ResourceId = serviceDiscovery.Id,
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

        private async Task<Amazon.ECS.Model.CreateServiceResponse> CreateEcsServiceAsync(EcsServiceDTO EcsService, EcsServiceConfig config, CancellationToken ct = default)
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
                TaskDefinition = EcsService.TaskDefinition,
                ServiceName = EcsService.ServiceName,
                EcsServiceRegistries = EcsService.Registries.Select(r => new Leadsly.Models.Aws.ElasticContainerService.EcsServiceRegistry
                {
                    RegistryArn = r.RegistryArn
                }).ToList()
            };

            return await _awsElasticContainerService.CreateServiceAsync(createEcsServiceRequest, ct);
        }

        private async Task<Amazon.ServiceDiscovery.Model.CreateServiceResponse> CreateServiceDiscoveryServiceAsync(CloudMapServiceDiscoveryServiceDTO cloudMapServiceDiscovery, CloudMapServiceDiscoveryConfig config, CancellationToken ct = default)
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
