using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class CloudMapServiceDiscoveryService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string ServiceDiscoveryId { get; set; }
        public string Arn { get; set; }
        public string Name { get; set; }
        public string NamespaceId { get; set; }
        public DateTime? CreateDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? EcsServiceId { get; set; }
        public EcsService EcsService { get; set; }
    }
}
