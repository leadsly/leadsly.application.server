﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class ConnectAccountTwoFactorAuth : ConnectAccount
    {
        public string Code { get; set; }
    }
}
