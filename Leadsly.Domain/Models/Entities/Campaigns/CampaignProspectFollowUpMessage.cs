using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities.Campaigns
{
    public class CampaignProspectFollowUpMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CampaignProspectFollowUpMessageId { get; set; }
        public string CampaignProspectId { get; set; }
        public CampaignProspect CampaignProspect { get; set; }
        public int Order { get; set; }
        public string Content { get; set; }
        public long ExpectedDeliveryDateTimeStamp { get; set; }

        [NotMapped]
        public DateTimeOffset ExpectedDeliveryDateTime { get; set; }
        public long ActualDeliveryDateTimeStamp { get; set; }
    }
}
