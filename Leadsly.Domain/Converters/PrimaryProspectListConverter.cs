using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Linq;

namespace Leadsly.Domain.Converters
{
    public static class PrimaryProspectListConverter
    {
        public static IList<UserProspectListViewModel> ConvertList(IList<PrimaryProspectList> models)
        {
            return models.Select(p =>
            {
                return new UserProspectListViewModel
                {
                    Name = p.Name
                };
            }).ToList();
        }
    }
}
