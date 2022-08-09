using Leadsly.Domain.Models.Entities.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leadsly.Infrastructure.Configurations
{
    public static class CampaignsConfiguration
    {
        public static void Configure(ModelBuilder builder, ILogger<DatabaseContext> logger)
        {
            logger.LogInformation("Configuring ecs resources database relationships.");

            builder.Entity<Campaign>(entity =>
            {
                entity
                .HasOne(x => x.FollowUpMessagePhase)
                .WithOne(z => z.Campaign)
                .OnDelete(DeleteBehavior.Cascade);

                entity
                .HasOne(x => x.ProspectListPhase)
                .WithOne(y => y.Campaign)
                .OnDelete(DeleteBehavior.Cascade);

                entity
                .HasMany(x => x.SearchUrlsProgress)
                .WithOne(y => y.Campaign)
                .OnDelete(DeleteBehavior.Cascade);

                entity
                .HasOne(x => x.SendConnectionRequestPhase)
                .WithOne(y => y.Campaign)
                .OnDelete(DeleteBehavior.Cascade);

                entity
                .HasMany(x => x.SendConnectionStages)
                .WithOne(y => y.Campaign)
                .OnDelete(DeleteBehavior.Cascade);

                entity
                .HasMany(x => x.SentConnectionsStatuses)
                .WithOne(y => y.Campaign)
                .OnDelete(DeleteBehavior.Cascade);

            });
        }
    }
}
