using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Models.Aws;

namespace Leadsly.Models
{
    [DataContract]
    public class CreateContainerDTO
    {
        [DataMember(IsRequired = true)]
        public string UserId { get; set; }

        [DataMember(IsRequired = true)]
        public string ServiceName { get; set; }

        [DataMember(IsRequired = true)]
        public string LaunchType { get; set; }

        [DataMember(IsRequired = true)]
        public NetworkConfigurationDTO NetworkConfiguration { get; set; }

        [DataMember(IsRequired = true)]
        public string SchedulingStrategy { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<Tag> Tags { get; set; } = new();
    }
}
