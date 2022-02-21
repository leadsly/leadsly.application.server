using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.Cloud
{
    [DataContract]
    public class SetupLeadslyViewModel
    {
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "socialAccountType")]
        public SocialAccountType SocialAccountType { get; set; }
    }
}
