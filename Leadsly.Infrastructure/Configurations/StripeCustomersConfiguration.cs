using Leadsly.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leadsly.Infrastructure.Configurations
{
    public static class StripeCustomersConfiguration
    {
        public static void Configure(ModelBuilder builder, ILogger<DatabaseContext> logger)
        {
            logger.LogInformation("Configuring stripe customers look up table.");

            builder.Entity<Customer_Stripe>().ToTable("StripeCustomers");
        }
    }
}
