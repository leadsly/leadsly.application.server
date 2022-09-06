using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class ChromeProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ChromeProfileId { get; set; }

        public string Name { get; set; }
        public PhaseType CampaignPhaseType { get; set; }
    }
}
