using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.ElasticContainerService
{
    public class RegisterEcsTaskDefinitionRequest
    {
        public List<string> RequiresCompatibilities { get; set; }
        /// <summary>
        /// The task definition name
        /// </summary>
        public string Family { get; set; }
        public List<EcsContainerDefinition> EcsContainerDefinitions { get; set; }
        public string Cpu { get; set; }
        public string Memory { get; set; }
        public string ExecutionRoleArn { get; set; }
        public string NetworkMode { get; set; } = "awsvpc";
    }
}
