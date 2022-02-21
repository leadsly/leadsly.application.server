using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.DTOs
{
    public class DnsConfigDTO
    {
        public List<CloudMapDnsRecordDTO> CloudMapDnsRecords { get; set; }
    }
}
