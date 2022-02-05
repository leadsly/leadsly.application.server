using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class LeadslySetupDTO
    {
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "socialAccountType")]
        public SocialAccountType SocialAccountType { get; set; }
    }
}
