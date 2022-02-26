using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Reports.ApexCharts
{
    [DataContract]
    public class ChartDataApexViewModel
    {
        [DataMember(Name = "campaignId", EmitDefaultValue = false, IsRequired = true)]
        public string CampaignId { get; set; }

        [DataMember(Name = "series", EmitDefaultValue = true, IsRequired = true)]
        public List<ApexAxisChartSeriesViewModel> Series { get; set; }

        [DataMember(Name = "xaxis", EmitDefaultValue = true, IsRequired = true)]
        public ApexXAxisViewModel XAxis { get; set; }
    }
}
