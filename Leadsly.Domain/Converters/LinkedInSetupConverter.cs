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
                UnexpectedErrorOccured = resp.UnexpectedErrorOccured
            };
        }
    }
}
