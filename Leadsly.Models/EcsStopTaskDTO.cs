using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class EcsStopTaskDTO
    {
        public string Reason { get; set; }
        public string Task { get; set; }
        public string Cluster { get; set; }
    }
}
