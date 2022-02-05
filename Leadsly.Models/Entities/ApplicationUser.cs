﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public string StripeCustomerId { get; set; }
        public bool Deleted { get; set; } = false;
        public string ExternalProviderUserId { get; set; }
        public string PhotoUrl { get; set; }
        public string ExternalProvider { get; set; }
        public Customer_Stripe Customer_Stripe { get; set; }
        public ICollection<DockerContainerInfo> DockerContainers { get; set; }
        public ICollection<SocialAccount> SocialAccounts { get; set; }
        public ICollection<Organization> Organizations { get; set; }
        public ICollection<Campaign> Campaigns { get; set; }
    }
}
