using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.ViewModels.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
