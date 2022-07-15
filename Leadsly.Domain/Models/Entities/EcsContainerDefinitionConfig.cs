﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models.Entities
{
    public class EcsContainerDefinitionConfig
    {
        public List<EcsPortMappingConfig> PortMappings { get; set; }
        public string Image { get; set; }
    }
}
