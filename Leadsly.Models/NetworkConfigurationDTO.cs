using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class NetworkConfigurationDTO
    {
        [DataMember(Name = "awsvpcConfiguration", IsRequired = true)]
        public AwsvpcConfiguration AwsvpcConfiguration { get; set; }
    }
}
