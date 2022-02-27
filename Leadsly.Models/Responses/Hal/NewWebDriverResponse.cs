using Leadsly.Models.Responses.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Responses.Hal
{
    [DataContract]
    public class NewWebDriverResponse : ResultBase, INewWebDriverResponse
    {
        [DataMember]
        public string WebDriverId { get; set; }
    }
}
