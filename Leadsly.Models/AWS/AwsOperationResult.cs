﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Aws
{
    public class AwsOperationResult
    {
        public bool Succeeded { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();
        public object Value { get; set; }
    }
}
