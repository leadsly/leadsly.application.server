using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Database
{
    public class ECSService
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceArn { get; set; }
        public string ClusterArn { get; set; }
        public long CreatedAt { get; set; }
        public long CreatedBy{ get; set; }        
        public string TaskDefinition { get; set; }
        public string RoleArn { get; set; }
        public ICollection<ECSTask> ECSTasks { get; set; }

    }
}
