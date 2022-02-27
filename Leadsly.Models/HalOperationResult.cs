using Leadsly.Models.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class HalOperationResult<T> : ResultBase
        where T : IOperationResponseViewModel
    {
        public T Value{ get; set; }
    }
}
