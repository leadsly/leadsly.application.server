using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Response.Hal
{
    public interface IConnectAccountResponseViewModel : IOperationResponseViewModel
    {
        public string WebDriverId { get; }
        public bool TwoFactorAuthRequired { get; }
        public TwoFactorAuthType TwoFactorAuthType { get; }
        public bool UnexpectedErrorOccured { get; }
    }
}
