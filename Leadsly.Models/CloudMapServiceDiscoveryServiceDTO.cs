using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class CloudMapServiceDiscoveryServiceDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DnsConfigDTO DnsConfig { get; set; }
        public string Arn { get; set; }
        public DateTime? CreateDate { get; set; }
        public string NamespaceId { get; set; }
        public string Description { get; set; }
        public string CreateRequestId { get; set; }
    }
}
