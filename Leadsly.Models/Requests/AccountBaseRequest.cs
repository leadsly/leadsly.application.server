﻿using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    [DataContract]
    public class AccountBaseRequest
    {
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "username", IsRequired = true)]
        public string Username { get; set; }

        [DataMember(Name = "socialAccountType", IsRequired = true)]
        public SocialAccountType SocialAccountType { get; set; }
    }
}
