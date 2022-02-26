using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Interfaces
{
    public interface IOperationResult
    {
        public bool Succeeded { get; set; }
        public List<Failure> Failures { get; set; }
    }
}
