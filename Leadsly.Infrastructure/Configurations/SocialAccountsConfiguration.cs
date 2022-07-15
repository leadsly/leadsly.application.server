using Leadsly.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leadsly.Infrastructure.Configurations
{
    public static class SocialAccountsConfiguration
    {
        public static void Configure(ModelBuilder builder, ILogger<DatabaseContext> logger)
        {
            logger.LogInformation("Configuring social account database relationships.");

            builder.Entity<SocialAccount>(entity =>
            {
                entity
                    .HasOne(sAcc => sAcc.VirtualAssistant)
                    .WithOne(res => res.SocialAccount)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
