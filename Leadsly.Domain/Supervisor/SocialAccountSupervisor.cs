using Leadsly.Domain.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<bool> PatchUpdateSocialAccountAsync(string socialAccountId, JsonPatchDocument<SocialAccount> updates, CancellationToken ct = default)
        {
            SocialAccount socialAccount = await _socialAccountRepository.GetByIdAsync(socialAccountId, ct);
            if (socialAccount == null)
            {
                _logger.LogError("Failed to retrieve social account by {socialAccountId}", socialAccountId);
                return true;
            }

            updates.ApplyTo(socialAccount);
            socialAccount = await _socialAccountRepository.UpdateAsync(socialAccount, ct);
            if (socialAccount == null)
            {
                _logger.LogError("Failed to apply updates to social account {socialAccountId}", socialAccountId);
                return false;
            }

            return true;
        }
    }
}
