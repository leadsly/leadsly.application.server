using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class EcsTaskDefinitionDTO
    {
        public string Family { get; set; }
        public string ContainerName { get; set; }
    }
}
