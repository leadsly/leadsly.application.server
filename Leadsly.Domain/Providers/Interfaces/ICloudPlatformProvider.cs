﻿using Leadsly.Domain.Models.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICloudPlatformProvider
    {
        Task<VirtualAssistant> GetVirtualAssistantAsync(string userId, CancellationToken ct = default);
        Task<Amazon.ServiceDiscovery.Model.RegisterInstanceResponse> RegisterCloudMapSrvInstanceAsync(string ecsServiceId, CancellationToken ct = default);
        Task<VirtualAssistant> CreateVirtualAssistantAsync(EcsTaskDefinition newEcsTaskDefinition, EcsService newEcsService, IList<EcsTask> ecsServiceTasks, CloudMapDiscoveryService newService, string halId, string userId, string timezoneId, CancellationToken ct = default);
        Task<bool> DeleteVirtualAssistantAsync(string virtualAssistantId, CancellationToken ct = default);
        Task<Amazon.ECS.Model.RegisterTaskDefinitionResponse> RegisterTaskDefinitionInAwsAsync(string taskDefinition, string halId, CancellationToken ct = default);
        Task<Amazon.ServiceDiscovery.Model.CreateServiceResponse> CreateCloudMapDiscoveryServiceInAwsAsync(string serviceDiscoveryName, CancellationToken ct = default);
        Task<Amazon.ECS.Model.CreateServiceResponse> CreateEcsServiceInAwsAsync(string serviceName, string taskDefinition, string cloudMapServiceArn, CancellationToken ct = default);
        Task<IList<EcsTask>> ListEcsServiceTasksAsync(string clusterArn, string serviceName, CancellationToken ct = default);
        public Task<bool> EnsureEcsServiceTasksAreRunningAsync(string ecsServiceName, string clusterArn, CancellationToken ct = default);
        Task DeleteAwsEcsServiceAsync(string userId, string serviceName, string clusterName, CancellationToken ct = default);
        Task DeleteAwsCloudMapServiceAsync(string userId, string serviceDiscoveryId, CancellationToken ct = default);
        Task DeleteAwsTaskDefinitionRegistrationAsync(string userId, string taskDefinitionFamily, CancellationToken ct = default);
        Task DeleteEcsServiceAsync(string ecsServiceId, CancellationToken ct = default);
        Task DeleteCloudMapServiceAsync(string cloudMapServiceId, CancellationToken ct = default);
        Task DeleteTaskDefinitionRegistrationAsync(string taskDefinitionId, CancellationToken ct = default);
    }
}
