using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models.ViewModels.Reports.ApexCharts
{
    [DataContract]
    public class ApexAxisChartSeriesViewModel
    {
        [DataMember(Name = "data", EmitDefaultValue = true, IsRequired = true)]
        public List<object> Data { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false, IsRequired = false)]
        public string Name { get; set; }
    }
}
