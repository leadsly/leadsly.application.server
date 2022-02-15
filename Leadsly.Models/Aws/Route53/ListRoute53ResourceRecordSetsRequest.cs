using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.Route53
{
    public class ListRoute53ResourceRecordSetsRequest
    {
        public string HostedZoneId { get; set; }
    }
}
