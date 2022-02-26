using Leadsly.Models;
using Leadsly.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Cloud
{
    public class SetupAccountResultViewModel
    {
        public bool Succeeded { get; set; }
        public bool RequiresNewCloudResource { get; set; }
        public bool NewUser { get; set; }
        public List<FailureViewModel> Failures { get; set; } = new();
    }
}
