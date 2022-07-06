namespace Leadsly.Domain.Models.ViewModels.LinkedInAccount
{
    public class TwoFactorAuthResultViewModel
    {
        public bool InvalidOrExpiredCode { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
        public bool FailedToEnterCode { get; set; }
    }
}
