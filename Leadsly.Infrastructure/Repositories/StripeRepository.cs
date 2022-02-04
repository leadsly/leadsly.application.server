using Leadsly.Domain.Repositories;
using Leadsly.Models.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class StripeRepository : IStripeRepository
    {
        public StripeRepository(ILogger<StripeRepository> logger, DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private readonly ILogger<StripeRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<Customer_Stripe> AddCustomerAsync(Customer_Stripe newStripeCustomer, CancellationToken ct = default)
        {
            _dbContext.StripeCustomers.Add(newStripeCustomer);
            await _dbContext.SaveChangesAsync(ct);

            return newStripeCustomer;
        }
    }
}
