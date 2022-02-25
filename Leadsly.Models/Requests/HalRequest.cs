using Leadsly.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    public class HalRequest : LeadslyBaseRequest
    {
        public string ServiceDiscoveryName { get; set; }
        public string NamespaceName { get; set; }
        public string PrivateIpAddress { get; set; }
        public string RequestUrl { get; set; }
    }
}
