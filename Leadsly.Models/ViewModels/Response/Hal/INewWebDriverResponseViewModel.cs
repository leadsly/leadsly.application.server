using Leadsly.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Response.Hal
{
    public interface INewWebDriverResponseViewModel : IOperationResponseViewModel
    {
        public string WebDriverId { get; }
    }
}
