using Leadsly.Application.Model.Entities;
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

            builder.Entity<VirtualAssistant>(entity =>
            {
                entity.HasOne(v => v.SocialAccount)
                .WithOne(s => s.VirtualAssistant)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.EcsService);
                entity.HasOne(v => v.EcsTaskDefinition);
                entity.HasOne(v => v.CloudMapDiscoveryService);
            });

            builder.Entity<CloudMapDiscoveryService>()
                    .HasOne(map => map.EcsService)
                    .WithOne(ser => ser.CloudMapDiscoveryService);

            builder.Entity<EcsService>(s =>
            {
                s.HasMany(s => s.EcsServiceRegistries)
                 .WithOne(reg => reg.EcsService)
                 .HasForeignKey(reg => reg.EcsServiceId)
                 .OnDelete(DeleteBehavior.Cascade);

                s.HasOne(s => s.CloudMapDiscoveryService)
                    .WithOne(map => map.EcsService)
                    .HasForeignKey<CloudMapDiscoveryService>(map => map.EcsServiceId);
            });

        }
    }
}
