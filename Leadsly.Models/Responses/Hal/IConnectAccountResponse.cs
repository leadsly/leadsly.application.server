using Leadsly.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Responses.Hal
{
    public interface IConnectAccountResponse : IOperationResponse
    {
        public string WebDriverId { get; }
        public bool TwoFactorAuthRequired { get; }
        public TwoFactorAuthType TwoFactorAuthType { get; }
        public bool UnexpectedErrorOccured { get; }
    }
}
