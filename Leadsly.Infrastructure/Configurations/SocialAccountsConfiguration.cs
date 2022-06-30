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
                    .HasOne(sAcc => sAcc.SocialAccountCloudResource)
                    .WithOne(res => res.SocialAccount)
                    .HasForeignKey<SocialAccountCloudResource>(res => res.SocialAccountId);
            });


            //builder.Entity<SocialAccountCloudResource>(res =>
            //{
            //    res.HasOne(res => res.EcsService).WithOne(ser => ser.SocialAccountCloudResource);
            //    res.HasOne(res => res.EcsTaskDefinition);
            //    res.HasOne(res => res.CloudMapDiscoveryService);
            //});

            builder.Entity<VirtualAssistant>(res =>
            {
                res.HasOne(res => res.EcsService);
                res.HasOne(res => res.EcsTaskDefinition);
                res.HasOne(res => res.CloudMapDiscoveryService);
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
