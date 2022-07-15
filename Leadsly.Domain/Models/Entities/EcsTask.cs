using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class EcsTask
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string EcsTaskId { get; set; }

        public string ContainerName { get; set; }
        public string EcsServiceId { get; set; }
        public ContainerPurpose ContainerPurpose { get; set; }
        public EcsService EcsService { get; set; }
    }
}
