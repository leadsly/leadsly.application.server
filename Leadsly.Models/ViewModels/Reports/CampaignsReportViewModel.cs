using Leadsly.Models.ViewModels.Reports.ApexCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Reports
{
    public class CampaignsReportViewModel
    {
        public DateRangeViewModel DateFilters { get; set; }
        public string SelectedCampaignId { get; set; }
        public List<ChartDataApexViewModel> Items { get; set; }        
    }
}
