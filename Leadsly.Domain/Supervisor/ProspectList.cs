using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    partial class Supervisor : ISupervisor
    {
        public async Task<HalOperationResultViewModel<T>> GetProspectListsByUserIdAsync<T>(string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            IList<PrimaryProspectList> prospectLists = await _campaignRepositoryFacade.GetPrimaryProspectListsByUserIdAsync(userId, ct);

            if (prospectLists == null)
            {
                return result;
            }

            IList<UserProspectListViewModel> userProspectLists = PrimaryProspectListConverter.ConvertList(prospectLists);

            result.OperationResults.Succeeded = true;
            result.Data = userProspectLists;
            return result;
        }
    }
}
