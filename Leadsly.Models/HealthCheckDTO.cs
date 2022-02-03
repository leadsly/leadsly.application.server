using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Leadsly
{
    [DataContract]
    public class HealthCheckDTO
    {
        /// <summary>
        /// HEALTHY | UNHEALTHY | UNKNOWN
        /// </summary>
        public string HealthStatus { get; set; }
    }
}
