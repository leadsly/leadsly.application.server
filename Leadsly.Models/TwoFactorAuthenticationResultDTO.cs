﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class TwoFactorAuthenticationResult
    {
        public bool Succeeded { get; set; } = false;
        public bool InvalidOrExpiredCode { get; set; } = false;
        public bool DidUnexpectedErrorOccur { get; set; } = false;
    }
}
