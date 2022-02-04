using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class LeadslySetupDTO
    {
        [DataMember(Name= "userId")]
        public string UserId { get; set; }
    }
}
