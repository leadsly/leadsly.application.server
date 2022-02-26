using Leadsly.Models.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests.Hal
{
    public interface INewWebDriverRequest : IHalRequest
    {
        public int DefaultTimeoutInSeconds { get; }
    }
}
