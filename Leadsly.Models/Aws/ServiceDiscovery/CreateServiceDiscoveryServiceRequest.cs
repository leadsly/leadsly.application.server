using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ServiceDiscovery
{
    public class CreateServiceDiscoveryServiceRequest
    {
        /// <summary>
        /// The name will be used to prepend the Aws Cloud Map namespace. For example if the name is hal-123 and the Aws Cloud Map namespace is leadsly-private, then 
        /// the url used to hit this service will be hal-123.leadsly-private.
        /// </summary>
        public string Name { get; set; }
        public string NamespaceId { get; set; }
        public CloudMapDnsConfig DnsConfig { get; set; }
    }
}
