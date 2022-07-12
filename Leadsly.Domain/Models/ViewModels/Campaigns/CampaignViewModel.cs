namespace Leadsly.Domain.Models.ViewModels.Campaigns
{
    public class CampaignViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long ConnectionsSentDaily { get; set; }
        public long TotalConnectionsSent { get; set; }
        public long ConnectionsAccepted { get; set; }
        public long Replies { get; set; }
        public long ProfileViews { get; set; }
        public bool Active { get; set; } = true;
        public bool Expired { get; set; } = false;
        public string Notes { get; set; }
    }
}
