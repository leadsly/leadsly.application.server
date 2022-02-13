using Amazon.ServiceDiscovery.Model;
using Leadsly.Models.Aws;
using Leadsly.Models.Aws.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
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
    }
}
