using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ServiceDiscovery
{
    public class CloudMapDnsRecord
    {
        /// <summary>
        /// Time to live
        /// </summary>
        public int TTL { get; set; } = 300;

        /// <summary>
        /// The type of DNS record this is. Default is A. Meaning only maps ip address to host name.
        /// </summary>
        public string Type { get; set; } = "A";
    }
}
