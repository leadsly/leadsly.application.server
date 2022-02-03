﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class ConnectAccountResult : WebDriverDetails
    {
        public bool Succeeded { get; set; } = false;
        public bool RequiresTwoFactorAuth { get; set; } = false;
        public bool DidUnexpectedErrorOccur { get; set; } = false;
        public TwoFactorAuthType AuthType { get; set; }
    }
}
