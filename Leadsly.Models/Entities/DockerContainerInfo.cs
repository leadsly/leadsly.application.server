using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class DockerContainerInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        [Required]
        public string EcsServiceId { get; set; }
        [Required]
        public string ContainerName { get; set; }
        public ICollection<SocialAccount> SocialAccounts { get; set; }
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
        [Required]
        public ECSService EcsService { get; set; }
    }
}
