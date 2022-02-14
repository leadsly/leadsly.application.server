using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Models.Entities;
using Leadsly.Models;

namespace Leadsly.Domain.Supervisor
{
    public interface ISupervisor
    {
        Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomerViewModel);
        Task<LeadslyConnectionResult> ConnectAccountToLeadslyAsync(ConnectLeadslyViewModel connectAccount, CancellationToken ct = default);
        Task<LeadslyConnectResultDTO> SetupLeadslyForUserAsync(ConnectUserDTO setup, CancellationToken ct = default);
    }
}
