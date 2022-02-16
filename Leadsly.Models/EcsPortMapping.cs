using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class EcsPortMapping
    {
        public string Protocol { get; set; } = "tcp";
        public int ContainerPort { get; set; }
    }
}
