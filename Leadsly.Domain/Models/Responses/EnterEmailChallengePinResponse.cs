namespace Leadsly.Domain.Models.Responses
{
    public class EnterEmailChallengePinResponse
    {
        public bool TwoFactorAuthRequired { get; set; }
        public bool InvalidOrExpiredPin { get; set; }
        public bool FailedToEnterPin { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
    }
}
