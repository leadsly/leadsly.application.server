using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class SocialAccountAndResourcesDTO
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public SocialAccountType AccountType { get; set; }
        public SocialAccountCloudResourceDTO Value { get; set; }        
    }
}
