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
    public class RequestNewWebDriverResultViewModel
    {
        public bool Succeeded { get; set; }        
        public IntantiateNewWebDriverResponseViewModel Value { get; set; }
        public List<FailureViewModel> Failures { get; set; } = new();
    }
}
