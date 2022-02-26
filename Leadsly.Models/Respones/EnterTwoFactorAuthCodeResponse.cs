using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Respones
{
    public class EnterTwoFactorAuthCodeResponse : LeadslyBaseResponse
    {
        public bool Succeeded { get; set; }
        public bool InvalidOrExpiredCode { get; set; }
        public bool DidUnexpectedErrorOccur { get; set; }        
        public List<FailureDTO> Failures { get; set; } = new();
    }
}
