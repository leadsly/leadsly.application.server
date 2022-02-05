using Amazon.ECS;
using Amazon.Runtime.Internal;
using Leadsly.Models;
using Leadsly.Models.Aws;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ECS.Model;

namespace Leadsly.Domain.Services
{
    public class AwsElasticContainerService : IAwsElasticContainerService
    {
        public AwsElasticContainerService(AmazonECSClient amazonClient, ILogger<AwsElasticContainerService> logger)
        {
            _amazonClient = amazonClient;
            _logger = logger;
        }

        private readonly AmazonECSClient _amazonClient;
        private readonly ILogger<AwsElasticContainerService> _logger;

        public async Task<CreateServiceResponse> CreateServiceAsync(CreateEcsServiceRequest createEcsServiceRequest, CancellationToken ct = default)
        {
            CreateServiceResponse resp = default;
            try
            {
                resp = await _amazonClient.CreateServiceAsync(new CreateServiceRequest
                {
                    DesiredCount = createEcsServiceRequest.DesiredCount,
                    ServiceName = createEcsServiceRequest.ServiceName,
                    TaskDefinition = createEcsServiceRequest.TaskDefinition,
                    Cluster = createEcsServiceRequest.Cluster,
                    LaunchType = createEcsServiceRequest.LaunchType,
                    NetworkConfiguration = new()
                    {
                        AwsvpcConfiguration = new()
                        {
                            AssignPublicIp = createEcsServiceRequest.AssignPublicIp                            
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
                resp = await _amazonClient.RunTaskAsync(new RunTaskRequest
                {
                    Cluster = runTaskRequest.Cluster,
                    Count = runTaskRequest.Count,
                    NetworkConfiguration = new()
                    {
                        AwsvpcConfiguration = new()
                        {
                            AssignPublicIp = runTaskRequest.AssignPublicIp,
                            Subnets = runTaskRequest.Subnets,
                            SecurityGroups = runTaskRequest.SecurityGroups
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
    }
}
