﻿using Leadsly.Models;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public interface IUserProvider
    {
        Task<SocialAccount> GetRegisteredSocialAccountAsync(SocialAccountDTO socialAccount, CancellationToken ct = default);
        Task<NewSocialAccountResult> AddUsersSocialAccountAsync(SocialAccountAndResourcesDTO newSocialAccountSetup, CancellationToken ct = default);
        Task<bool> RemoveSocialAccountAndResourcesAsync(SocialAccount socialAccount, CancellationToken ct = default);


    }
}
