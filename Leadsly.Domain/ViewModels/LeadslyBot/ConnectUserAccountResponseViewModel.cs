using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.LeadslyBot
{
    public class ConnectUserAccountResponseViewModel
    {        
        public bool Succeeded { get; set; }        
        public string WebDriverId { get; set; }        
        public bool TwoFactorAuthRequired { get; set; }        
        public TwoFactorAuthType TwoFactorAuthType { get; set; }        
        public bool UnexpectedErrorOccured { get; set; }
        public List<FailureViewModel> Failures { get; set; } = new();
    }
}
