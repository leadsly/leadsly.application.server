using Leadsly.Domain.ViewModels.Reports.ApexCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.Reports
{
    public class CampaignsReportViewModel
    {
        public DateRangeViewModel DateFilters { get; set; }
        public string SelectedCampaignId { get; set; }
        public List<ChartDataApexViewModel> Items { get; set; }        
    }
}
