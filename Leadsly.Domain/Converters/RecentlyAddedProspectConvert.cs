using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.MonitorForNewConnections;
using System.Collections.Generic;
using System.Linq;

namespace Leadsly.Domain.Converters
{
    public static class RecentlyAddedProspectConvert
    {
        public static IList<RecentlyAddedProspectModel> ConvertList(IEnumerable<RecentlyAddedProspect> entities)
        {
            return entities.Select(entity =>
            {
                return new RecentlyAddedProspectModel()
                {
                    AcceptedRequestTimestamp = entity.AcceptedRequestTimestamp,
                    Name = entity.Name,
                    ProfileUrl = entity.ProfileUrl
                };
            }).ToList();
        }

        public static IList<RecentlyAddedProspect> ConvertList(IEnumerable<RecentlyAddedProspectModel> items)
        {
            return items.Select(item =>
            {
                return new RecentlyAddedProspect()
                {
                    AcceptedRequestTimestamp = item.AcceptedRequestTimestamp,
                    Name = item.Name,
                    ProfileUrl = item.ProfileUrl
                };
            }).ToList();
        }
    }
}
