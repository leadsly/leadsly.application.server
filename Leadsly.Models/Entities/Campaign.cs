using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class Campaign
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string ApplicationUserId { get; set; }
        public string Name { get; set; }
        public long ConnectionsSentDaily { get; set; }
        public long TotalConnectionsSent { get; set; }
        public long ConnectionsAccepted { get; set; }
        public long Replies { get; set; }
        public long ProfileViews { get; set; }
        public bool Active { get; set; } = true;
        public bool Expired { get; set; } = false;
        public string Notes { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
