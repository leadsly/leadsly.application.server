using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using System.Threading.Tasks;

namespace Leadsly.Domain.JobServices
{
    public class RestartHalJobService : IRestartHalJobService
    {
        public RestartHalJobService(IRestartHalService service)
        {
            _service = service;
        }

        private readonly IRestartHalService _service;

        public async Task RestartHalAsync(string halId)
        {
            await _service.RestartAsync(halId);
        }
    }
}
