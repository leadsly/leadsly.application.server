﻿using Amazon.ServiceDiscovery.Model;
using Leadsly.Application.Model.Aws.ServiceDiscovery;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IAwsServiceDiscoveryService
    {
        /// <summary>
        /// Creates a service discovery service. The name of this service is what AWS uses to figure out what private ip address is assigned to that name.
        /// </summary>
        /// <param name="createServiceDiscoveryRequest"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CreateServiceResponse> CreateServiceAsync(CreateServiceDiscoveryServiceRequest createServiceDiscoveryRequest, CancellationToken ct = default);

        Task<DeleteServiceResponse> DeleteServiceAsync(DeleteServiceDiscoveryServiceRequest deleteServiceDiscoveryRequest, CancellationToken ct = default);

        Task<string> RollbackCloudMapDiscoveryServiceAsync(CancellationToken ct = default);

        Task<GetNamespaceResponse> GetNamespaceAsync(GetCloudMapNamespaceRequest getNamespaceRequest, CancellationToken ct = default);
    }
}
