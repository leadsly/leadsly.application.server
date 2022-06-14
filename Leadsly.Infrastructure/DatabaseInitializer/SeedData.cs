using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Leadsly.Application.Model.Entities;
using System.Linq;

namespace Leadsly.Infrastructure.DatabaseInitializer
{
    public class SeedData
    {
        public static void Populate(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            DatabaseContext dbContext = serviceProvider.GetService<DatabaseContext>();

            Seed(dbContext, logger);
        }

        private static IList<string> InitialTimeZoneIds = new List<string>()
        {
            "Eastern Standard Time",
            "Central Standard Time",
            "Mountain Standard Time",
            "Pacific Standard Time",
            "Alaskan Standard Time",
            "Hawaiian Standard Time"
        };

        private static void Seed(DatabaseContext dbContext, ILogger<DatabaseInitializer> logger)
        {
            // populate database with UnitedStates timezones
            if(dbContext.SupportedTimeZones.Count() == 0)
            {
                foreach (string tzId in InitialTimeZoneIds)
                {
                    LeadslyTimeZone tz = new LeadslyTimeZone
                    {
                        TimeZoneId = tzId
                    };
                    dbContext.SupportedTimeZones.Add(tz);
                    dbContext.SaveChanges();
                }
            }            
        }
    }
}
