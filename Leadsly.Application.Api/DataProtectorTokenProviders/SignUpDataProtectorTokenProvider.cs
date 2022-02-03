using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.DataProtectorTokenProviders
{
    public class SignUpDataProtectorTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public SignUpDataProtectorTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<SignUpDataProtectionTokenProviderOptions> options, ILogger<SignUpDataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        private readonly SignUpDataProtectionTokenProviderOptions _options;
        private readonly ILogger<DataProtectorTokenProvider<TUser>> _logger;
    }
}
