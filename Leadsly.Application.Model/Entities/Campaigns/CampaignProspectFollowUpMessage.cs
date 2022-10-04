using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class CampaignProspectFollowUpMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CampaignProspectFollowUpMessageId { get; set; }
        public string CampaignProspectId { get; set; }
        public CampaignProspect CampaignProspect { get; set; }
        public int? Order { get; set; } = null;
        public string Content { get; set; }
    }
}
