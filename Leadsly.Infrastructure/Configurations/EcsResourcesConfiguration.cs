using Leadsly.Domain.Models;
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

            ValueConverter<ContainerPurpose, string> converter = new ValueConverter<ContainerPurpose, string>(v => v.ToString(), v => (ContainerPurpose)Enum.Parse(typeof(ContainerPurpose), v));

            builder.Entity<EcsTask>()
                .Property(s => s.ContainerPurpose)
                .HasConversion(converter);
        }
    }
}
