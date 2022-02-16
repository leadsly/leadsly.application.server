using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class PortMappingDTO
    {
        public int ContainerPort { get; set; }
        public string Protocol { get; set; } = "tcp";
    }
}
