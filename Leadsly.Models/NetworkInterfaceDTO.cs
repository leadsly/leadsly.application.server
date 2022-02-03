using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class NetworkInterfaceDTO
    {
        [DataMember(Name = "ipv6Address")]
        public string Ipv6Address { get; set; }
        [DataMember(Name = "privateIpv4Address")]
        public string PrivateIpv4Address { get; set; }
    }
}
