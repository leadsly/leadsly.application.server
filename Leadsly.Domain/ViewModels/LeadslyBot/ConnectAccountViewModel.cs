﻿using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.LeadslyBot
{
    [DataContract]
    public class ConnectAccountViewModel
    {
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "username", IsRequired = true)]
        public string Username { get; set; }

        [DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; }

        [DataMember(Name = "socialAccountType", IsRequired = true)]
        public SocialAccountType SocialAccountType { get; set; }
    }
}
