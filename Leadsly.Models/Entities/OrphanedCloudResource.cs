using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class OrphanedCloudResource
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
                
        public string Arn { get; set; }

        [Required]
        public string FriendlyName { get; set; }
        [Required]
        public string ResourceId { get; set; }
        [Required]
        public string UserId { get; set; }
        public string ResourceName { get; set; }
        public string Reason { get; set; }

    }
}
