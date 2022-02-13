using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class EcsContainerDefinition
    {
        public string Name { get; set; }
        public List<EcsPortMapping> PortMappings { get; set; }
        public string Image { get; set; }
    }
}
