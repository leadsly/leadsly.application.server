﻿using Amazon.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class EcsPortMapping
    {
        public int ContainerPort { get; set; }
        public string Protocol { get; set; } = "tcp";
    }
}
