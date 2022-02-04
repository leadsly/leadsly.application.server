using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Models.Entities;

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
