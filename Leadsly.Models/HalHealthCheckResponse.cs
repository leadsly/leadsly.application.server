using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class HalHealthCheckResponse
    {
        public bool Succeeded { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();
        public HalHealthCheck Value { get; set; }
    }
}
