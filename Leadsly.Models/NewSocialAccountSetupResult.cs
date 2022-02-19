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
        public bool IsHalHealthy { get; set; }
        public bool Succeeded { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public SocialAccountType AccountType { get; set; }
        public SocialAccountCloudResourceDTO Value { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();

    }
}
