using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels
{
    public interface IOperationResultViewModel
    {
        public bool Succeeded { get; set; }
        public List<FailureViewModel> Failures { get; set; }
    }
}
