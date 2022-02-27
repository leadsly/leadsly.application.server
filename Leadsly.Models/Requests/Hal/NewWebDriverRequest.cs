using Leadsly.Models;
using Leadsly.Models.Requests;
using Leadsly.Models.Requests.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests.Hal
{
    public class NewWebDriverRequest : BaseRequest, INewWebDriverRequest
    {
        public int DefaultTimeoutInSeconds { get; set; }
    }
}
