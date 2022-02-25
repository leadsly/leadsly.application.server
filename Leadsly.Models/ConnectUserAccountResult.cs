using Leadsly.Models.Respones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class ConnectUserAccountResult
    {
        public bool Succeeded { get; set; }
        public ConnectUserAccountResponse Value { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();
    }
}
