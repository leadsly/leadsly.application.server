using Leadsly.Domain.Models;
using Leadsly.Models.ViewModels;
using Leadsly.Models.ViewModels.Hal;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Models.Entities;
using Leadsly.Models;
using Leadsly.Models.ViewModels.Cloud;
using Leadsly.Models.ViewModels.Interfaces;

namespace Leadsly.Domain.Supervisor
{
    public interface ISupervisor
    {
        Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomerViewModel);
        Task<SetupAccountResultViewModel> LeadslyAccountSetupAsync(SetupAccountViewModel setup, CancellationToken ct = default);        
        Task<HalOperationResult<T>> LeadslyRequestNewWebDriverAsync<T>(RequestNewWebDriverViewModel request, CancellationToken ct = default)
            where T : IOperationResponse;        
        Task<HalOperationResult<T>> LeadslyAuthenticateUserAsync<T>(ConnectAccountViewModel connect, CancellationToken ct = default)
            where T : IOperationResponse;
        Task<HalOperationResult<T>> LeadslyTwoFactorAuthAsync<T>(TwoFactorAuthViewModel twoFactorAuth, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
