using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class FailureDTO
    { 
        public string ResourceId { get; set; }             
        public string Arn { get; set; }        
        public string Detail { get; set; }        
        public string Reason { get; set; }
    }
}
