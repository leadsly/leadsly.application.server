using Leadsly.Models.Requests.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    public class NewWebDriverRequest : HalRequestBase, INewWebDriverRequest
    {
        public int DefaultTimeoutInSeconds { get; set; }
    }
    
}
