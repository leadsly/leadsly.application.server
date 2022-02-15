using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class HalRequest
    {
        public string DiscoveryServiceName { get; set; }
        public string NamespaceName { get; set; }
        public string PrivateIpAddress { get; set; }
        public string RequestUrl { get; set; }
    }
}
