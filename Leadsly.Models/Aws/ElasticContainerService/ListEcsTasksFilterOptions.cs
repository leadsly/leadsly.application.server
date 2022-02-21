using Amazon.ECS;
using Leadsly.Models.Aws.DTOs;
using Leadsly.Models.Aws.ElasticContainerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class ListEcsTasksFilterOptions
    {
        public string Cluster { get; set; }
        public string ContainerInstance { get; set; }
        public EcsDesiredStatus DesiredStatus { get; set; }
        public string Family { get; set; }
        public EcsLaunchType LaunchType { get; set; }
        public string ServiceName { get; set; }
    }
}
