using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Leadsly.Domain.Converters
{
    public static class TimeZoneConverter
    {
        public static IList<TimeZoneViewModel> ConvertList(IEnumerable<LeadslyTimeZone> supportedTimeZones)
        {
            return supportedTimeZones.Select(tz =>
            {
                return new TimeZoneViewModel
                {
                    TimeZoneId = tz.TimeZoneId
                };
            }).ToList();
        }
    }
}
