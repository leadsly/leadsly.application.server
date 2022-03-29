using Leadsly.Api;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("prospect-list")]
    public class ProspectListController : ApiControllerBase
    {
        public ProspectListController(ISupervisor supervisor)
        {
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ProspectList(ProspectListPhaseCompleteRequest request, CancellationToken ct = default)
        {
            var a  = await _supervisor.ProcessProspectsAsync<IOperationResponse>(request, ct);

            return Ok();
        }
    }
}
