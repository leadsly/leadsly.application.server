namespace Leadsly.Domain.Models.Responses
{
    public class EnterTwoFactorAuthResponse
    {
        public bool InvalidOrExpiredCode { get; set; }
        public bool FailedToEnterCode { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
    }
}
