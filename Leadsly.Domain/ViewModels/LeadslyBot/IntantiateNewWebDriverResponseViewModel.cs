using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.LeadslyBot
{
    [DataContract]
    public class IntantiateNewWebDriverResponseViewModel
    {
        public bool Succeeded { get; set; }        
        public string WebDriverId { get; set; }
    }
}
