using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Models.Entities;
using Leadsly.Models;
using Leadsly.Domain.ViewModels.Cloud;

namespace Leadsly.Domain.Supervisor
{
    public interface ISupervisor
    {
        Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomerViewModel);
        Task<SetupAccountResultViewModel> LeadslyAccountSetupAsync(SetupAccountViewModel setup, CancellationToken ct = default);
        Task<RequestNewWebDriverResultViewModel> LeadslyRequestNewWebDriverAsync(RequestNewWebDriverViewModel request, CancellationToken ct = default);
        Task<ConnectAccountResultViewModel> LeadslyAuthenticateUserAsync(ConnectAccountViewModel connect, CancellationToken ct = default);
        Task<TwoFactorAuthResultViewModel> LeadslyTwoFactorAuthAsync(TwoFactorAuthViewModel twoFactorAuth, CancellationToken ct = default);
    }
}
