using Leadsly.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests.Hal
{
    public class HealthCheckRequest : BaseRequest
    {
        public string PrivateIpAddress { get; set; }
    }
}
