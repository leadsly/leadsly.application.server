using Leadsly.Application.Model;

namespace Leadsly.Domain.Models.ViewModels.LinkedInAccount
{
    public class ConnectLinkedInAccountResultViewModel
    {
        public bool TwoFactorAuthRequired { get; set; }
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
        public bool InvalidEmail { get; set; }
        public bool InvalidPassword { get; set; }
    }
}
