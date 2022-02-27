using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Models.Entities;
using Leadsly.Models;
using Leadsly.Models.ViewModels.Cloud;
using Leadsly.Models.Requests;
using Leadsly.Models.ViewModels.Response;
using Leadsly.Models.Responses;
using Leadsly.Models.ViewModels;

namespace Leadsly.Domain.Supervisor
{
    public interface ISupervisor
    {
        Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomerViewModel);
        Task<SetupAccountResultViewModel> LeadslyAccountSetupAsync(SetupAccountViewModel setup, CancellationToken ct = default);   
        
        Task<HalOperationResultViewModel<T>> LeadslyRequestNewWebDriverAsync<T>(NewWebDriverRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;
        Task<HalOperationResultViewModel<T>> LeadslyAuthenticateUserAsync<T>(ConnectAccountRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;
        Task<HalOperationResultViewModel<T>> LeadslyTwoFactorAuthAsync<T>(TwoFactorAuthRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;
    }
}
