using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class EcsTaskDefinition
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string EcsTaskDefinitionId { get; set; }
        [Required]
        public string TaskDefinitionArn { get; set; }
        [Required]
        public string Family { get; set; }
        [Required]
        public string HalId { get; set; }
        [Required]
        public EcsResourcePurpose Purpose { get; set; }
    }
}
