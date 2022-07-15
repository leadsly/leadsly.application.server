using System.Collections.Generic;

namespace Leadsly.Domain.Models.Entities
{
    public class EcsContainerDefinitionConfig
    {
        public List<EcsPortMappingConfig> PortMappings { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
    }
}
