using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models.Respones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class HalAuthenticationConverter
    {
        public static EnterTwoFactorAuthCodeResponseViewModel Convert(EnterTwoFactorAuthCodeResponse resp)
        {
            return new EnterTwoFactorAuthCodeResponseViewModel
            {
                DidUnexpectedErrorOccur = resp.DidUnexpectedErrorOccur,
                Failures = FailureConverter.ConvertList(resp.Failures),
                InvalidOrExpiredCode = resp.InvalidOrExpiredCode,
                Succeeded = resp.Succeeded
            };
        }

        public static ConnectUserAccountResponseViewModel Convert(ConnectUserAccountResponse resp)
        {
            return new ConnectUserAccountResponseViewModel
            {
                UnexpectedErrorOccured = resp.UnexpectedErrorOccured,
                Failures = FailureConverter.ConvertList(resp.Failures),
                Succeeded = resp.Succeeded,
                TwoFactorAuthRequired = resp.TwoFactorAuthRequired,
                TwoFactorAuthType = resp.TwoFactorAuthType,
                WebDriverId = resp.WebDriverId
            };
        }

        public static IntantiateNewWebDriverResponseViewModel Convert(IntantiateNewWebDriverResponse resp)
        {
            return new IntantiateNewWebDriverResponseViewModel
            {
                Succeeded = resp.Succeeded,
                WebDriverId = resp.WebDriverId
            };
        }
    }
}
