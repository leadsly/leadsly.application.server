using Leadsly.Domain.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IStripeRepository
    {
        Task<Customer_Stripe> AddCustomerAsync(Customer_Stripe newStripeCustomer, CancellationToken ct = default);
    }
}
