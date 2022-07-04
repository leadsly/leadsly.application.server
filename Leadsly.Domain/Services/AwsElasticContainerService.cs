using Amazon.ECS;
using Amazon.ECS.Model;
using Leadsly.Application.Model.Aws.ElasticContainerService;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class AwsElasticContainerService : IAwsElasticContainerService
    {
        public AwsElasticContainerService(AmazonECSClient amazonEcsClient, ILogger<AwsElasticContainerService> logger)
        {
            _amazonEcsClient = amazonEcsClient;
            _logger = logger;
        }
        private readonly AmazonECSClient _amazonEcsClient;
        private readonly ILogger<AwsElasticContainerService> _logger;

        public async Task<CreateServiceResponse> CreateServiceAsync(CreateEcsServiceRequest createEcsServiceRequest, CancellationToken ct = default)
        {
            CreateServiceResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.CreateServiceAsync(new CreateServiceRequest
                {
                    DesiredCount = createEcsServiceRequest.DesiredCount,
                    ServiceName = createEcsServiceRequest.ServiceName,
                    TaskDefinition = createEcsServiceRequest.TaskDefinition,
                    Cluster = createEcsServiceRequest.Cluster,
                    LaunchType = createEcsServiceRequest.LaunchType,
                    ServiceRegistries = createEcsServiceRequest.EcsServiceRegistries.Select(r => new ServiceRegistry
                    {
                        RegistryArn = r.RegistryArn
                    }).ToList(),
                    NetworkConfiguration = new()
                    {
                        AwsvpcConfiguration = new()
                        {
                            AssignPublicIp = createEcsServiceRequest.AssignPublicIp,
                            Subnets = createEcsServiceRequest.Subnets,
                            SecurityGroups = createEcsServiceRequest.SecurityGroups
                        }
                    },
                    SchedulingStrategy = createEcsServiceRequest.SchedulingStrategy
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create new AWS service.");
            }

            return resp;
        }

        public async Task<bool> DeleteServiceAsync(string serviceName, string clusterName, CancellationToken ct = default)
        {
            bool result = false;
            if (string.IsNullOrEmpty(serviceName) == false && string.IsNullOrEmpty(clusterName) == false)
            {
                Amazon.ECS.Model.DeleteServiceResponse response = await DeleteServiceAsync(new DeleteEcsServiceRequest
                {
                    Service = serviceName,
                    Cluster = clusterName,
                    Force = true
                }, ct);

                result = response != null;
            }

            return result;
        }

        public async Task<RunTaskResponse> RunTaskAsync(RunEcsTaskRequest runTaskRequest, CancellationToken ct = default)
        {
            RunTaskResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.RunTaskAsync(new RunTaskRequest
                {
                    Cluster = runTaskRequest.ClusterArn,
                    Count = runTaskRequest.Count,
                    NetworkConfiguration = new()
                    {
                        AwsvpcConfiguration = new()
                        {
                            AssignPublicIp = runTaskRequest.AssignPublicIp,
                            Subnets = runTaskRequest.Subnets
                        }
                    },
                    LaunchType = runTaskRequest.LaunchType,
                    TaskDefinition = runTaskRequest.TaskDefinition
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create AWS task.");
            }

            return resp;
        }

        public async Task<StopTaskResponse> StopTaskAsync(StopEcsTaskRequest stopTaskRequest, CancellationToken ct = default)
        {
            StopTaskResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.StopTaskAsync(new StopTaskRequest
                {
                    Cluster = stopTaskRequest.Cluster,
                    Reason = stopTaskRequest.Reason,
                    Task = stopTaskRequest.Task
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop ecs task.");
            }

            return resp;
        }

        public async Task<UpdateServiceResponse> UpdateServiceAsync(UpdateEcsServiceRequest updateServiceRequest, CancellationToken ct = default)
        {
            UpdateServiceResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.UpdateServiceAsync(new UpdateServiceRequest
                {
                    Cluster = updateServiceRequest.ClusterArn,
                    DesiredCount = updateServiceRequest.DesiredCount,
                    Service = updateServiceRequest.ServiceName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update AWS service.");
            }

            return resp;
        }

        public async Task<DescribeServicesResponse> DescribeServicesAsync(DescribeEcsServicesRequest describeServiceRequest, CancellationToken ct = default)
        {
            DescribeServicesResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.DescribeServicesAsync(new DescribeServicesRequest
                {
                    Cluster = describeServiceRequest.Cluster,
                    Services = describeServiceRequest.Services
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update AWS service.");
            }

            return resp;
        }

        public async Task<RegisterTaskDefinitionResponse> RegisterTaskDefinitionAsync(RegisterEcsTaskDefinitionRequest registerTaskDefinitionRequest, CancellationToken ct = default)
        {
            RegisterTaskDefinitionResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.RegisterTaskDefinitionAsync(new RegisterTaskDefinitionRequest
                {
                    RequiresCompatibilities = registerTaskDefinitionRequest.RequiresCompatibilities,
                    Family = registerTaskDefinitionRequest.Family,
                    ContainerDefinitions = registerTaskDefinitionRequest.EcsContainerDefinitions.Select(c => new ContainerDefinition
                    {
                        Name = c.Name,
                        Image = c.Image,
                        Environment = c.EnviornmentVariables.Select(var => new KeyValuePair
                        {
                            Name = var.Key,
                            Value = var.Value
                        }).ToList()
                    }).ToList(),
                    Cpu = registerTaskDefinitionRequest.Cpu,
                    Memory = registerTaskDefinitionRequest.Memory,
                    TaskRoleArn = registerTaskDefinitionRequest.TaskRoleArn,
                    ExecutionRoleArn = registerTaskDefinitionRequest.ExecutionRoleArn,
                    NetworkMode = registerTaskDefinitionRequest.NetworkMode,
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register new task definition.");
            }

            return resp;
        }

        public async Task<bool> DeleteTaskDefinitionRegistrationAsync(string taskDefinitionFamily, CancellationToken ct = default)
        {
            bool result = false;
            if (string.IsNullOrEmpty(taskDefinitionFamily) == false)
            {
                DeregisterTaskDefinitionResponse response = await DeregisterTaskDefinitionAsync(new DeregisterEcsTaskDefinitionRequest
                {
                    TaskDefinition = taskDefinitionFamily
                }, ct);

                result = response == null;
            }

            return result;
        }

        public async Task<DeregisterTaskDefinitionResponse> DeregisterTaskDefinitionAsync(DeregisterEcsTaskDefinitionRequest deregisterTaskDefinitionRequest, CancellationToken ct = default)
        {
            DeregisterTaskDefinitionResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.DeregisterTaskDefinitionAsync(new DeregisterTaskDefinitionRequest
                {
                    TaskDefinition = deregisterTaskDefinitionRequest.TaskDefinition
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deregister task definition.");
            }

            return resp;
        }

        public async Task<DeleteServiceResponse> DeleteServiceAsync(DeleteEcsServiceRequest deleteServiceRequest, CancellationToken ct = default)
        {
            DeleteServiceResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.DeleteServiceAsync(new DeleteServiceRequest
                {
                    Cluster = deleteServiceRequest.Cluster,
                    Force = deleteServiceRequest.Force,
                    Service = deleteServiceRequest.Service
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete ecs service.");
            }

            return resp;
        }

        public async Task<ListTasksResponse> ListTasksAsync(ListEcsTasksRequest listEcsTasksRequest, CancellationToken ct = default)
        {
            ListTasksResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.ListTasksAsync(new ListTasksRequest
                {
                    Cluster = listEcsTasksRequest.Cluster,
                    ContainerInstance = listEcsTasksRequest.ContainerInstance,
                    DesiredStatus = listEcsTasksRequest.DesiredStatus,
                    Family = listEcsTasksRequest.Family,
                    LaunchType = listEcsTasksRequest.LaunchType,
                    ServiceName = listEcsTasksRequest.ServiceName
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve list of ecs tasks.");
            }

            return resp;
        }

        public async Task<DescribeTasksResponse> DescribeTasksAsync(DescribeEcsTasksRequest describeEcsTasksRequest, CancellationToken ct = default)
        {
            DescribeTasksResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.DescribeTasksAsync(new DescribeTasksRequest
                {
                    Cluster = describeEcsTasksRequest.Cluster,
                    Tasks = describeEcsTasksRequest.Tasks
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve details about ecs tasks.");
            }

            return resp;
        }

        public async Task<DescribeServicesResponse> DescribeServices(DescribeEcsServicesRequest describeEcsServicesRequest, CancellationToken ct = default)
        {
            DescribeServicesResponse resp = default;
            try
            {
                resp = await _amazonEcsClient.DescribeServicesAsync(new DescribeServicesRequest
                {
                    Cluster = describeEcsServicesRequest.Cluster,
                    Services = describeEcsServicesRequest.Services
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve details about ecs services.");
            }

            return resp;
        }
    }
}
