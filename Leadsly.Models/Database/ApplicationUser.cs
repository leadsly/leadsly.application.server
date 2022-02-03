﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Database
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
    }
}
