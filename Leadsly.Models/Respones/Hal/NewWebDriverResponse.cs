using Leadsly.Models.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Respones.Hal
{
    [DataContract]
    public class NewWebDriverResponse : ResultBase, INewWebDriverResponse
    {
        [DataMember]
        public string WebDriverId { get; set; }
    }
}
