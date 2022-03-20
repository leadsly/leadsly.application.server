using Amazon.Route53;
using Amazon.Route53.Model;
using Leadsly.Application.Model.Aws.Route53;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class AwsRoute53Service : IAwsRoute53Service
    {
        public AwsRoute53Service(AmazonRoute53Client amazonClient, ILogger<AwsRoute53Service> logger)
        {
            _amazonClient = amazonClient;
            _logger = logger;
        }

        private readonly AmazonRoute53Client _amazonClient;
        private readonly ILogger<AwsRoute53Service> _logger;

        public async Task<ListResourceRecordSetsResponse> ListResourceRecordSetsAsync(ListRoute53ResourceRecordSetsRequest request, CancellationToken ct = default)
        {
            ListResourceRecordSetsResponse resp = default;
            try
            {
                resp = await _amazonClient.ListResourceRecordSetsAsync(new ListResourceRecordSetsRequest
                {
                    HostedZoneId = request.HostedZoneId
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to get list of resource record sets from aws.");
            }
            return resp;
        }
    }
}
