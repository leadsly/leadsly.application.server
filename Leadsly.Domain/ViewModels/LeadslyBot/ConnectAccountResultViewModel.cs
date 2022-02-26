using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.LeadslyBot
{
    public class ConnectAccountResultViewModel
    {
        public bool Succeeded { get; set; }
        public ConnectUserAccountResponseViewModel Value { get; set; }
        public List<FailureViewModel> Failures { get; set; } = new();
    }
}
