using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class Enter2faAuthenticationCodeDTO
    {
        [DataMember(IsRequired = true)]
        public string Code { get; set; }
    }
}
