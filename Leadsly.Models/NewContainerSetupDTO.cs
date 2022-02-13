using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class NewContainerSetupDTO
    {
        public EcsTaskDefinitionDTO EcsTaskDefinition { get; set; }
        public CloudMapServiceDiscoveryDTO CloudMapServiceDiscovery { get; set; }
        public EcsServiceDTO EcsService { get; set; }
    }
}
