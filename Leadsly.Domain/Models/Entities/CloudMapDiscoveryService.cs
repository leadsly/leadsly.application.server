using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Domain.Models.Entities
{
    public class CloudMapDiscoveryService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CloudMapDiscoveryServiceId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ServiceDiscoveryId { get; set; }
        public string Arn { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string NamespaceId { get; set; }
        [Required]
        public string HalId { get; set; }
        [Required]
        public EcsResourcePurpose Purpose { get; set; }
        [Required]
        public DateTime? CreateDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string EcsServiceId { get; set; }
        public EcsService EcsService { get; set; }
    }
}
