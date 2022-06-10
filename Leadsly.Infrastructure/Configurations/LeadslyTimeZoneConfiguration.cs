using Leadsly.Application.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leadsly.Infrastructure.Configurations
{
    public static class LeadslyTimeZoneConfiguration
    {
        public static void Configure(ModelBuilder builder, ILogger<DatabaseContext> logger)
        {
            builder.Entity<LeadslyTimeZone>(entity =>
            {
                entity.HasIndex(e => e.TimeZoneId).IsUnique();
            });
        }
    }
}
