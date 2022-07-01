using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<ConnectedViewModel> GetConnectedAccountAsync(string userId, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = await _cloudPlatformProvider.GetVirtualAssistantAsync(userId, ct);
            if (virtualAssistant == null)
            {
                return new ConnectedViewModel
                {
                    IsConnected = false,
                    ConnectedAccount = null
                };
            }
            else
            {
                return new ConnectedViewModel
                {
                    IsConnected = virtualAssistant.SocialAccount == null ? false : virtualAssistant.SocialAccount.Linked,
                    ConnectedAccount = virtualAssistant.SocialAccount == null ? null : virtualAssistant.ToConnectedAccountViewModel()
                };
            }
        }
    }
}
