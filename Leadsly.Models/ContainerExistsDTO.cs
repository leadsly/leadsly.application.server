using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class ContainerExistsDTO
    {
        public bool Exists { get; set; }
        public List<string> ContainerIds { get; set; }
    }
}
