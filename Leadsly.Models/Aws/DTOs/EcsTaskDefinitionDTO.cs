using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.DTOs
{
    public class EcsTaskDefinitionDTO
    {
        public List<string> RequiresCompatibilities { get; set; }
        public string TaskDefinitionArn { get; set; }
        public string Family { get; set; }
        public string Cpu { get; set; }
        public List<ContainerDefinitionDTO> ContainerDefinitions { get; set; }
        public string Memory { get; set; }
        public string ExecutionRoleArn { get; set; }
        public string TaskRoleArn { get; set; }
        public string NetworkMode { get; set; }
        public string ContainerName { get; set; }
    }
}
