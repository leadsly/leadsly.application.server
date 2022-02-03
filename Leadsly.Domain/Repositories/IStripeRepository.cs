using Leadsly.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IStripeRepository
    {
        Task<Customer_Stripe> AddCustomerAsync(Customer_Stripe newStripeCustomer, CancellationToken ct = default);
    }
}
