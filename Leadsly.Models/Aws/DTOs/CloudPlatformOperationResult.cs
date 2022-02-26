﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws.DTOs
{
    public class CloudPlatformOperationResult
    {
        public bool Succeeded { get; set; }
        public List<Failure> Failures { get; set; } = new();
        public object Value { get; set; }
    }
}
