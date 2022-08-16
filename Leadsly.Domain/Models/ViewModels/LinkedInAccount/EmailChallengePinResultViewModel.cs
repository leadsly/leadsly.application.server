namespace Leadsly.Domain.Models.ViewModels.LinkedInAccount
{
    public class EmailChallengePinResultViewModel
    {
        public bool TwoFactorAuthRequired { get; set; }
        public bool InvalidOrExpiredPin { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
        public bool FailedToEnterPin { get; set; }
    }
}
