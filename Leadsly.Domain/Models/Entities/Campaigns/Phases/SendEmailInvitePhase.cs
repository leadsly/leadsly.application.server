using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models.Entities.Campaigns.Phases
{
    public class SendEmailInvitePhase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SendEmailInvitePhaseId { get; set; }
        public string CampaignId { get; set; }
        public PhaseType PhaseType { get; set; }
        public Campaign Campaign { get; set; }

    }
}
