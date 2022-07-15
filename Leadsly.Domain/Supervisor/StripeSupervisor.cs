using Leadsly.Domain.Models.Entities;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomer)
        {
            stripeCustomer = await _stripeRepository.AddCustomerAsync(stripeCustomer);

            return stripeCustomer;
        }
    }
}
