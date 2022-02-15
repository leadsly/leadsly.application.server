using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class DescribeEcsServiceRequest
    {
        public string Cluster { get; set; }
        public List<string> Services { get; set; }
    }
}
