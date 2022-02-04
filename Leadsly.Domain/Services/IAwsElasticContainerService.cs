using Leadsly.Models;
using Leadsly.Models.Aws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public interface IAwsElasticContainerService
    {
        Task<CreateServiceResponse> CreateServiceAsync(CreateServiceRequest request, CancellationToken ct = default);

        Task<RunTaskResponse> RunTaskAsync(RunTaskRequest request, CancellationToken ct = default);
    }
}
