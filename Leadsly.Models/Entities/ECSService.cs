using Leadsly.Models.Aws.ElasticContainerService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class EcsService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? SocialAccountCloudResourceId { get; set; }
        [Required]
        public string UserId { get; set; }        
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
        public string TaskDefinition { get; set; }
        [Required]
        public int DesiredCount { get; set; }
        [Required]
        public string SchedulingStrategy { get; set; }
        [Required]
        public string AssignPublicIp { get; set; }
        public CloudMapServiceDiscoveryService CloudMapServiceDiscoveryService { get; set; }
        public SocialAccountCloudResource SocialAccountCloudResource { get; set; }
        public ICollection<EcsServiceRegistry> EcsServiceRegistries { get; set; }

    }
}
