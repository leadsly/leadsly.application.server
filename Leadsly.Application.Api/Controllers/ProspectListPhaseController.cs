using Leadsly.Api;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProspectListPhaseController : ApiControllerBase
    {
        public ProspectListPhaseController(ILogger<ProspectListPhaseController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ILogger<ProspectListPhaseController> _logger;
        private readonly ISupervisor _supervisor;

        [HttpPatch("{prospectListPhaseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateProspectList(string prospectListPhaseId, [FromBody] JsonPatchDocument<ProspectListPhase> patchDoc, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing UpdateProspectListPhase action for ProspectListPhaseId {prospectListPhaseId}", prospectListPhaseId);
            HalOperationResult<IOperationResponse> result = await _supervisor.UpdateProspectListPhaseAsync<IOperationResponse>(prospectListPhaseId, patchDoc, ct);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to update PropspectListPhase with id {prospectListPhaseId}", prospectListPhaseId);
                return BadRequest_UpdatingProspectListPhase(result.Failures);
            }

            return Ok();
        }
    }
}
