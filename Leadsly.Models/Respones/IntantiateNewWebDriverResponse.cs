﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Respones
{
    public class IntantiateNewWebDriverResponse : LeadslyBaseResponse
    {
        public bool Succeeded { get; set; }
        public string WebDriverId { get; set; }
    }
}
