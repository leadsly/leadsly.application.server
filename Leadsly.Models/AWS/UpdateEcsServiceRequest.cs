using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws
{
    public class UpdateEcsServiceRequest
    {
        public string Cluster { get; set; }
        public string Service { get; set; }
        public int DesiredCount { get; set; }
    }
}
