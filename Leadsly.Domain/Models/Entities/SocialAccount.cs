using Leadsly.Application.Model;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class SocialAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SocialAccountId { get; set; }
        [Required]
        public string UserId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string VirtualAssistantId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string HalUnitId { get; set; }
        public SocialAccountType AccountType { get; set; }
        [Required]
        public string Username { get; set; }
        public bool RunProspectListFirst { get; set; } = false;
        public bool MonthlySearchLimitReached { get; set; } = false;

        /// <summary>
        /// Whether this social account is linked with virtual assistant
        /// </summary>
        public bool Linked { get; set; } = false;
        public ApplicationUser User { get; set; }
        public VirtualAssistant VirtualAssistant { get; set; }
        public HalUnit HalDetails { get; set; }
        public ScanProspectsForRepliesPhase ScanProspectsForRepliesPhase { get; set; }
        public ConnectionWithdrawPhase ConnectionWithdrawPhase { get; set; }
        public MonitorForNewConnectionsPhase MonitorForNewProspectsPhase { get; set; }
    }
}
