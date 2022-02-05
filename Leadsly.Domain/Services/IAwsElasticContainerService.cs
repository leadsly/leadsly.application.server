using Amazon.ECS.Model;
using Leadsly.Models;
using Leadsly.Models.Aws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public interface IAwsElasticContainerService
    {
        Task<CreateServiceResponse> CreateServiceAsync(CreateEcsServiceRequest createServiceRequest, CancellationToken ct = default);

        Task<RunTaskResponse> RunTaskAsync(RunEcsTaskRequest request, CancellationToken ct = default);
    }
}
