using Leadsly.Domain.Services;
using Leadsly.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class AwsElasticContainerProvider : IAwsElasticContainerProvider
    {
        public AwsElasticContainerProvider(IAwsElasticContainerService awsElasticContainerService, ILogger<AwsElasticContainerProvider> logger)
        {
            _awsElasticContainerService = awsElasticContainerService;
            _logger = logger;
        }

        private readonly IAwsElasticContainerService _awsElasticContainerService;
        private readonly ILogger<AwsElasticContainerProvider> _logger;

        public Task<CreateContainerResultDTO> SetupUsersContainer()
        {
            throw new NotImplementedException();
        }
    }
}
