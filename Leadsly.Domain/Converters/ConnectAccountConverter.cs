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
                UnexpectedErrorOccured = response.UnexpectedErrorOccured,
                InvalidOrExpiredCode = response.InvalidOrExpiredCode,
                Failures = FailureConverter.ConvertList(response.OperationInformation.Failures)
            };
        }

        public static IConnectAccountResponseViewModel Convert(IConnectAccountResponse response)
        {
            return new ConnectResponseViewModel
            {
                TwoFactorAuthRequired = response.TwoFactorAuthRequired,
                TwoFactorAuthType = response.TwoFactorAuthType,
                UnexpectedErrorOccured = response.UnexpectedErrorOccured,
                Failures = FailureConverter.ConvertList(response.OperationInformation.Failures)
            };
        }

    }
}
