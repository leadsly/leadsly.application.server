using Leadsly.Models;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public interface ILeadslyProvider
    {
        Task<SocialAccount> GetSocialAccountAsync(SocialAccountDTO getSocialAccount, CancellationToken ct = default);
        // Task<DockerContainerInfo> GetContainerInfoBySocialAccountAsync(SocialAccountDTO socialAccount, CancellationToken ct = default);
    }
}
