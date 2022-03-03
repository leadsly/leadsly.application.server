using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class ConnectAccountConverter
    {
        public static IEnterTwoFactorAuthCodeResponseViewModel Convert(IEnterTwoFactorAuthCodeResponse response)
        {
            return new EnterTwoFactorAuthCodeResponseViewModel
            {
                DidUnexpectedErrorOccur = response.DidUnexpectedErrorOccur,
                InvalidOrExpiredCode = response.InvalidOrExpiredCode                
            };
        }

        public static IConnectAccountResponseViewModel Convert(IConnectAccountResponse response)
        {
            return new ConnectResponseViewModel
            {
                TwoFactorAuthRequired = response.TwoFactorAuthRequired,
                TwoFactorAuthType = response.TwoFactorAuthType,
                UnexpectedErrorOccured = response.UnexpectedErrorOccured                
            };
        }

    }
}
