﻿using Amazon.ECS;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ECS.Model;
using Leadsly.Models.Aws.ElasticContainerService;
using System.Linq;

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
            catch(Exception ex)
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
                        PortMappings = c.PortMappings.Select(p => new PortMapping
                        {
                            ContainerPort = p.ContainerPort,
                            Protocol = new TransportProtocol(p.Protocol)
                        }).ToList(),
                        Image = c.Image
                    }).ToList(),
                    Cpu = registerTaskDefinitionRequest.Cpu,
                    Memory = registerTaskDefinitionRequest.Memory,
                    ExecutionRoleArn = registerTaskDefinitionRequest.ExecutionRoleArn,
                    RuntimePlatform = new()
                    {
                        OperatingSystemFamily = OSFamily.LINUX
                    },
                    NetworkMode = registerTaskDefinitionRequest.NetworkMode,
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register new task definition.");
            }

            return resp;
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
    }
}
