using Leadsly.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using System;

namespace Leadsly.Infrastructure.Configurations
{
    public static class EcsResourcesConfiguration
    {
        public static void Configure(ModelBuilder builder, ILogger<DatabaseContext> logger)
        {
            logger.LogInformation("Configuring ecs resources database relationships.");

            builder.Entity<VirtualAssistant>(entity =>
            {
                entity.HasOne(v => v.SocialAccount)
                .WithOne(s => s.VirtualAssistant)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.EcsServices);
                entity.HasMany(v => v.EcsTaskDefinitions);
                entity.HasMany(v => v.CloudMapDiscoveryServices);
            });

            builder.Entity<CloudMapDiscoveryService>()
                    .HasOne(map => map.EcsService)
                    .WithOne(ser => ser.CloudMapDiscoveryService);

            ValueConverter<Purpose, string> converter = new ValueConverter<Purpose, string>(v => v.ToString(), v => (Purpose)Enum.Parse(typeof(Purpose), v));

            builder.Entity<EcsService>(s =>
            {
                s.Property(s => s.Purpose)
                .HasConversion(converter);

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
