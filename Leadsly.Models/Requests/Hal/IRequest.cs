using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests.Hal
{
    public interface IRequest
    {
        public string ServiceDiscoveryName { get; }
        public string NamespaceName { get; }
        public string RequestUrl { get; }
    }
}
