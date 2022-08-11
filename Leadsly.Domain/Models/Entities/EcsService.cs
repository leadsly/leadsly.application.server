using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class EcsService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string EcsServiceId { get; set; }
        [Required]
        public string ServiceName { get; set; }
        [Required]
        public string ServiceArn { get; set; }
        [Required]
        public string ClusterArn { get; set; }
        [Required]
        public long CreatedAt { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public Purpose Purpose { get; set; }
        [Required]
        public string TaskDefinition { get; set; }
        [Required]
        public string SchedulingStrategy { get; set; }
        public CloudMapDiscoveryService CloudMapDiscoveryService { get; set; }
        public ICollection<EcsServiceRegistry> EcsServiceRegistries { get; set; }
        public ICollection<EcsTask> EcsTasks { get; set; }

    }

    public enum Purpose
    {
        None,
        Grid,
        Hal
    }
}
