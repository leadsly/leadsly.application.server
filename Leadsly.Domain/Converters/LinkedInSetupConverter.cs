using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;

namespace Leadsly.Domain.Converters
{
    public static class LinkedInSetupConverter
    {
        public static ConnectLinkedInAccountResultViewModel Convert(ConnectLinkedInAccountResponse resp)
        {
            return new ConnectLinkedInAccountResultViewModel
            {
                TwoFactorAuthRequired = resp.TwoFactorAuthRequired,
                TwoFactorAuthType = resp.TwoFactorAuthType,
                InvalidEmail = resp.InvalidEmail,
                InvalidPassword = resp.InvalidPassword,
                UnexpectedErrorOccured = resp.UnexpectedErrorOccured
            };
        }

        public static TwoFactorAuthResultViewModel Convert(EnterTwoFactorAuthResponse resp)
        {
            return new TwoFactorAuthResultViewModel
            {
                InvalidOrExpiredCode = resp.InvalidOrExpiredCode,
                FailedToEnterCode = resp.FailedToEnterCode,
                UnexpectedErrorOccured = resp.UnexpectedErrorOccured
            };
        }
    }
}
