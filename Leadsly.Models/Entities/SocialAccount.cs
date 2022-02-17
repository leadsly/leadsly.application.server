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
        public string Id { get; set; }
        [Required]
        public string UserId { get; set; }      
        public SocialAccountType AccountType { get; set; }        
        [Required]
        public string Username { get; set; }
        [Required]
        public bool ConfiguredWithUsersLeadslyAccount { get; set; }
        public SocialAccountCloudResource SocialAccountCloudResource { get; set; }
        public ApplicationUser User { get; set; }
    }
}
