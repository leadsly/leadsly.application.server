using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class NewSocialAccountSetupResult
    {
        public bool CreateEcsTaskDefinitionSucceeded { get; set; }
        public bool CreateServiceDiscoveryServiceSucceeded { get; set; }
        public bool CreateEcsServiceSucceeded { get; set; }
        public bool Succeeded { get; set; }
        public NewContainerSetupDTO Value { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();

    }
}
