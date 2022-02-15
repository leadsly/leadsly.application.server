using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class DescribeEcsServiceResponse
    {
        public bool Succeeded { get; set; }
        public Amazon.ECS.Model.Service Service { get; set; }
        public List<FailureDTO> Failures { get; set; }
    }
}
