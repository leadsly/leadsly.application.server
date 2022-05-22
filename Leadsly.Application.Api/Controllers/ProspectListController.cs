using Leadsly.Api;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests.FromHal;
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
    public class ProspectListController : ApiControllerBase
    {
        public ProspectListController(ILogger<ProspectListController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ILogger<ProspectListController> _logger;
        private readonly ISupervisor _supervisor;

        [HttpPost("{halId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ProspectList(string halId, CollectedProspectsRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing ProspectList action for HalId {halId}", halId);
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessProspectsAsync<IOperationResponse>(request, ct);

            if(result.Succeeded == false)
            {
                _logger.LogDebug("Failed to process prospects for HalId {halId}", halId);
                return BadRequest_ProspectList(result.Failures);
            }

            return Ok();
        }
    }
}
