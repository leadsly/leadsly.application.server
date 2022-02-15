using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class HalHealthCheck
    {
        public string ApiVersion { get; set; }
        public string HalsUniqueName { get; set; }
        public string Status { get; set; }

    }
}
