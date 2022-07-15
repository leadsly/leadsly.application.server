﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models.Entities.Campaigns.Phases
{
    public class ConnectionWithdrawPhase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ConnectionWithdrawPhaseId { get; set; }
        public PhaseType PhaseType { get; set; }
        public string PageUrl { get; set; } = "https://www.linkedin.com/mynetwork/invitation-manager/sent/";
        public string SocialAccountId { get; set; }
        public SocialAccount SocialAccount { get; set; }
    }
}
