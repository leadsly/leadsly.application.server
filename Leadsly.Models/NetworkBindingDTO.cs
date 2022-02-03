using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class NetworkBindingDTO
    {
        public string BindIP { get; set; }
        public long ContainerPort { get; set; }
        public long HostPort { get; set; }
        /// <summary>
        /// The protocol used for the network binding.
        /// Valid Values: tcp | udp
        /// </summary>
        public string Protocol { get; set; }
    }
}
