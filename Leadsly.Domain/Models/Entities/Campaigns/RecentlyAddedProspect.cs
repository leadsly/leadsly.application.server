using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities.Campaigns
{
    public class RecentlyAddedProspect
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string RecentlyAddedProspectId { get; set; }
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
        public long AcceptedRequestTimestamp { get; set; }
        public string SocialAccountId { get; set; }
    }
}
