using Leadsly.Application.Model.Aws.Route53;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IAwsRoute53Service
    {
        Task<Amazon.Route53.Model.ListResourceRecordSetsResponse> ListResourceRecordSetsAsync(ListRoute53ResourceRecordSetsRequest requeste, CancellationToken ct = default);
    }
}
