﻿namespace Leadsly.Domain.Models.Networking
{
    public class SearchUrlProgressModel
    {
        public string SearchUrlProgressId { get; set; }
        public string WindowHandleId { get; set; }
        public int LastPage { get; set; }
        public int LastProcessedProspect { get; set; }
        public string SearchUrl { get; set; }
        public bool StartedCrawling { get; set; }
        public int TotalSearchResults { get; set; }
        public bool Exhausted { get; set; }
        public string CampaignId { get; set; }
        public long LastActivityTimestamp { get; set; }
    }
}
