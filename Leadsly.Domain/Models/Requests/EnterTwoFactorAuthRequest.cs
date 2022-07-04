using Leadsly.Application.Model.WebDriver;

namespace Leadsly.Domain.Models.Requests
{
    public class EnterTwoFactorAuthRequest
    {
        public string RequestUrl { get; set; }
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string Code { get; set; }
        public string WindowHandleId { get; set; } = string.Empty;
        public BrowserPurpose BrowserPurpose { get; set; } = BrowserPurpose.Auth;
        public long AttemptNumber { get; set; } = 1;
    }
}
