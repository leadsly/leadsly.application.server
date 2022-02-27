using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Response.Hal
{
    public class ConnectResponseViewModel : ResultBaseViewModel, IConnectAccountResponseViewModel
    {
        public string WebDriverId { get; set; }

        public bool TwoFactorAuthRequired { get; set; }

        public TwoFactorAuthType TwoFactorAuthType { get; set; }

        public bool UnexpectedErrorOccured { get; set; }
    }
}
