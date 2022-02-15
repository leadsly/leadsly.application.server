﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class SocialAccountCloudResource
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;
        public string SocialAccountId { get; set; }
        public string ContainerName { get; set; }
        public SocialAccount SocialAccount { get; set; }
        public string EcsServiceId { get; set; }
        public EcsService EcsService { get; set; }
        public string EcsTaskDefinitionId { get; set; }
        public EcsTaskDefinition EcsTaskDefinition { get; set; }
        public string CloudMapServiceDiscoveryServiceId { get; set; }
        public CloudMapServiceDiscoveryService CloudMapServiceDiscoveryService { get; set; }        
    }
}
