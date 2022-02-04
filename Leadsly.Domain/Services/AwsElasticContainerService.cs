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

namespace Leadsly.Domain.Services
{
    public class AwsElasticContainerService : IAwsElasticContainerService
    {
        public AwsElasticContainerService(HttpClient httpClient, ILogger<AwsElasticContainerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<AwsElasticContainerService> _logger;

        public Task<CreateServiceResponse> CreateServiceAsync(CreateServiceRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<RunTaskResponse> RunTaskAsync(RunTaskRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
