﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Domain.OptionsJsonModels
{
    public class UserRegistrationOptions
    {
        public class Token
        {
            public int LifeSpanInDays { get; }
        }
    }
}
