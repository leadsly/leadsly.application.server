using Leadsly.Application.Model.WebDriver;

namespace Leadsly.Domain.Models.Requests
{
    public class AuthenticateLinkedInAccountRequest
    {
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public BrowserPurpose BrowserPurpose { get; set; } = BrowserPurpose.Auth;
        public long AttemptNumber { get; set; }
        public string ConnectAuthUrl { get; set; } = "https://linkedin.com";
    }
}
