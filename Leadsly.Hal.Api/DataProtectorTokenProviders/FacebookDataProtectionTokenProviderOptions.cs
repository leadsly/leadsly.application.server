using Microsoft.AspNetCore.Identity;

namespace Leadsly.Hal.Api.DataProtectorTokenProviders
{
    public class FacebookDataProtectionTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
