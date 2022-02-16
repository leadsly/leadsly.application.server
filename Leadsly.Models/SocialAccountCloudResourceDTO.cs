using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class SocialAccountCloudResourceDTO
    {
        public string ContainerName { get; set; }
        public EcsTaskDefinitionDTO EcsTaskDefinition { get; set; }
        public CloudMapServiceDiscoveryServiceDTO CloudMapServiceDiscovery { get; set; }
        public EcsServiceDTO EcsService { get; set; }
    }
}
