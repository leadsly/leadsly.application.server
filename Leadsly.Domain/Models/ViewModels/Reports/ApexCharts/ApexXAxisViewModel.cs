using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models.ViewModels.Reports.ApexCharts
{
    [DataContract]
    public class ApexXAxisViewModel
    {
        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = true)]
        public string Type { get; set; }

        [DataMember(Name = "categories", EmitDefaultValue = false, IsRequired = true)]
        public List<string> Categories { get; set; }
    }
}
