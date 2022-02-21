using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.DTOs
{
    public class ContainerDefinitionDTO
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public List<PortMappingDTO> PortMappings { get; set; }
        public List<KeyValuePair<string, string>> EnviornmentVariables { get; set; }
    }
}
