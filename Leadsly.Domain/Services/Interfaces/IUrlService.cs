using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IUrlService
    {
        public string GetHalsBaseUrl(string namespaceName);
    }
}
