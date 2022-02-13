﻿using Leadsly.Models.Aws;
using Leadsly.Models.Aws.ElasticContainerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class EcsTaskDTO
    {
        public EcsLaunchType LaunchType { get; set; }

        public PublicIp AssignPublicIp { get; set; }
        public string TaskDefinition { get; set; }
        public string Cluster { get; set; }
        public int Count { get; set; }
        public List<string> Subnets { get; set; }
    }
}
