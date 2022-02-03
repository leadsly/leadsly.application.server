using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class ContainerDTO
    {
        public string ContainerArn { get; set; }
        public string CPU { get; set; }
        public long ExitCode { get; set; }
        /// <summary>
        /// The health status of the container. If health checks aren't configured for this container in its task definition, then it reports the health status as UNKNOWN.
        /// Valid Values: HEALTHY | UNHEALTHY | UNKNOWN
        /// </summary>
        public string HealthStatus { get; set; }
        /// <summary>
        /// The image used for the container.
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// The name of the container.
        /// </summary>
        public string Name { get; set; }

        public List<NetworkBindingDTO> NetworkBindings { get; set; }
        public List<NetworkInterfaceDTO> NetworkInterfaces { get; set; }

        /// <summary>
        /// A short (255 max characters) human-readable string to provide additional details about a running or stopped container.
        /// </summary>
        public string Reason { get; set; }

        public string TaskArn { get; set; }
    }
}
