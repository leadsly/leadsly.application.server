using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    public class LeadslyBaseRequest
    {
        public string ServiceDiscoveryName { get; set; }
        public string NamespaceName { get; set; }
    }
}
