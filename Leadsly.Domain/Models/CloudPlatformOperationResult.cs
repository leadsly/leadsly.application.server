using Leadsly.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Models
{
    public class CloudPlatformOperationResult
    {
        public bool Succeeded { get; set; }
        public List<Failure> Failures { get; set; } = new();
        public object Value { get; set; }
    }
}
