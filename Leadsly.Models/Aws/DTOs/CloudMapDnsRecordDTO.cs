using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.DTOs
{
    public class CloudMapDnsRecordDTO
    {
        public int TTL { get; set; }
        public string Type { get; set; }
    }
}
