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
    public class ECSService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
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
        public long CreatedBy{ get; set; }
        [Required]
        public string TaskDefinition { get; set; }
        [Required]
        public string RoleArn { get; set; }
        public ICollection<ECSTask> ECSTasks { get; set; }

    }
}
