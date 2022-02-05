﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class ConnectAccountDTO : WebDriverDetailsDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public SocialAccountType AccountType { get; set; }        
    }
}
