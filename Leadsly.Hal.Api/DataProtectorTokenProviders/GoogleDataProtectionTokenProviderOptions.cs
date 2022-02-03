using Microsoft.AspNetCore.Identity;

namespace Leadsly.Hal.Api.DataProtectorTokenProviders
{
    public class GoogleDataProtectionTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
