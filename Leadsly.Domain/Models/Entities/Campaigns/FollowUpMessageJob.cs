using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models.Entities.Campaigns
{
    public class FollowUpMessageJob
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FollowUpMessageJobId { get; set; }

        public string CampaignProspectId { get; set; }
        public string FollowUpMessageId { get; set; }
        public string HangfireJobId { get; set; }
    }
}
