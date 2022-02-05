using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class SocialAccountDTO
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public SocialAccountType AccountType { get; set; }
    }
}
