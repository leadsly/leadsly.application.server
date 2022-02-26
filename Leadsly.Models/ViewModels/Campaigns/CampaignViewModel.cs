using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Campaigns
{
    [DataContract]
    public class CampaignViewModel
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "connectionsSentDaily")]
        public long ConnectionsSentDaily { get; set; }
        [DataMember(Name = "totalConnectionsSent")]
        public long TotalConnectionsSent { get; set; }
        [DataMember(Name = "connectionsAccepted")]
        public long ConnectionsAccepted { get; set; }
        [DataMember(Name = "replies")]
        public long Replies { get; set; }
        [DataMember(Name = "profileViews")]
        public long ProfileViews { get; set; }
        [DataMember(Name = "active")]
        public bool Active { get; set; } = true;
        [DataMember(Name = "expired")]
        public bool Expired { get; set; } = false;
        [DataMember(Name = "notes")]
        public string Notes { get; set; }
    }
}
