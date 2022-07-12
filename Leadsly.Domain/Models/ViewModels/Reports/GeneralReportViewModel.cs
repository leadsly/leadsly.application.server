using Leadsly.Domain.Models.ViewModels.Reports.ApexCharts;
using System.Collections.Generic;

namespace Leadsly.Domain.Models.ViewModels.Reports
{
    public class GeneralReportViewModel
    {
        public DateRangeViewModel DateFilters { get; set; }
        public string SelectedCampaignId { get; set; }
        public List<ChartDataApexViewModel> Items { get; set; }
    }
}
