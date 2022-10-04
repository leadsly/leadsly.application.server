using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class FollowUpMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FollowUpMessageId { get; set; }
        public string Content { get; set; }
        public int? Order { get; set; } = null;
        public FollowUpMessageDelay Delay { get; set; }
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}
