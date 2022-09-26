using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class VirtualAssistant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string VirtualAssistantId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public string HalId { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string HalUnitId { get; set; }
        public bool Provisioned { get; set; } = false;
        public SocialAccount SocialAccount { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public IList<EcsService> EcsServices { get; set; }
        public IList<EcsTaskDefinition> EcsTaskDefinitions { get; set; }
        public IList<CloudMapDiscoveryService> CloudMapDiscoveryServices { get; set; }
        public HalUnit HalUnit { get; set; }
    }
}
