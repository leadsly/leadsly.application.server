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
                Failures = FailureConverter.ConvertList(response.Failures),
                InvalidOrExpiredCode = response.InvalidOrExpiredCode,
                Succeeded = response.Succeeded
            };
        }

        public static IConnectAccountResponseViewModel Convert(IConnectAccountResponse response)
        {
            return new ConnectResponseViewModel
            {
                Failures = FailureConverter.ConvertList(response.Failures),
                Succeeded= response.Succeeded,
                TwoFactorAuthRequired = response.TwoFactorAuthRequired,
                TwoFactorAuthType = response.TwoFactorAuthType,
                UnexpectedErrorOccured = response.UnexpectedErrorOccured,
                WindowHandleId = response.WindowHandleId
            };
        }

    }
}
