using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.LeadslyBot
{
    public class EnterTwoFactorAuthCodeResponseViewModel
    {
        public bool Succeeded { get; set; }
        public bool InvalidOrExpiredCode { get; set; }
        public bool DidUnexpectedErrorOccur { get; set; }
        public List<FailureViewModel> Failures { get; set; } = new();
    }
}
