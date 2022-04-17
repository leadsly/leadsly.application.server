using Leadsly.Api;
using Leadsly.Application.Model;
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
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessProspectsAsync<IOperationResponse>(request, ct);

            if(result.Succeeded == false)
            {
                return BadRequest_ProspectListPhase(result.Failures);
            }

            return Ok();
        }

        [HttpPost("prospects-replied")]
        [AllowAnonymous]
        public async Task<IActionResult> CampaignProspectsReplied(ProspectsRepliedRequest request, CancellationToken ct = default)
        {
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessProspectsRepliedAsync<IOperationResponse>(request, ct);

            if (result.Succeeded == false)
            {
                return BadRequest_CampaignProspectsReplied(result.Failures);
            }

            return Ok();
        }
    }
}
