using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Models.Aws;

namespace Leadsly.Models
{
    public class SetupNewUserInLeadslyDTO
    {

        public string UserId { get; set; }
        public ECSServiceDTO EcsService { get; set; }
        public ECSTaskDTO EcsTask { get; set; }
    }
}
