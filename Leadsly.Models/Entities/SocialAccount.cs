using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class SocialAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [NotMapped]
        public bool DockerContainerCreated { get; set; } = false;
        [NotMapped]
        public bool DuplicateSocialAccountsFound { get; set; }
        [Required]
        public long CreatedAtTimestamp { get; set; }
        public SocialAccountType AccountType { get; set; }
        [Required]
        public string ContainerId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public bool Connected { get; set; }
        public DockerContainerInfo DockerContainerInfo { get; set; }
    }
}
