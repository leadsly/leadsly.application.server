using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class ECSTask
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string ECSServiceId { get; set; }
        public string TaskDefinition { get; set; }
        public string AssignPublicIp { get; set; }
        public int Count { get; set; }
        public string LaunchType { get; set; }        
        public string UserId { get; set; }
        public ECSService ECSService { get; set; }
        public ICollection<DockerContainerInfo> DockerContainers { get; set; }
    }
}
