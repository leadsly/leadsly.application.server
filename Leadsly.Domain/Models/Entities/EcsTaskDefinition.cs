using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class EcsTaskDefinition
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string EcsTaskDefinitionId { get; set; }
        public string TaskDefinitionArn { get; set; }
        public string Family { get; set; }
    }
}
