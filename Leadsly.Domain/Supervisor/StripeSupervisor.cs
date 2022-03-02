using Leadsly.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Application.Model.Entities;

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
