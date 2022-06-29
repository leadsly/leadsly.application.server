using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Converters;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<HalOperationResultViewModel<T>> GetConnectedAccountAsync<T>(string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            SocialAccount socialAccount = await _socialAccountRepository.GetByUserIdAsync(userId, ct);

            if (socialAccount != null)
            {
                SocialAccountViewModel socialAccountViewModel = SocialAccountConverter.Convert(socialAccount);

                ConnectedAccountViewModel viewModel = new()
                {
                    ConnectedAccount = socialAccountViewModel?.Username,
                    HalId = socialAccountViewModel?.HalId
                };

                result.Data = viewModel;
            }

            result.OperationResults.Succeeded = true;
            return result;
        }
    }
}
