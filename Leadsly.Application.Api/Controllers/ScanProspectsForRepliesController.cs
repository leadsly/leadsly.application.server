using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class ScanProspectsForRepliesController : ApiControllerBase
    {
        public ScanProspectsForRepliesController(ILogger<ScanProspectsForRepliesController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<ScanProspectsForRepliesController> _logger;

        [HttpPost]
        [Route("{halId}")]
        public async Task<IActionResult> CreateScanProspectsForRepliesMessageAsync(string halId, [FromBody] TriggerScanProspectsForRepliesRequest request)
        {
            _logger.LogInformation("Executing CreateScanProspectsForRepliesMessage action for HalId {halId}", halId);
            await _supervisor.TriggerScanProspectsForRepliesPhaseAsync(request);
            return Ok();
        }

        [HttpPost("{halId}/prospects-replied")]
        public async Task<IActionResult> ProspectsReplied(string halId, ProspectsRepliedRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing ProspectsReplied action for HalId {halId}", halId);

            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessProspectsRepliedAsync<IOperationResponse>(request, ct);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to process prospects that have replied for HalId {halId}", halId);
                return BadRequest_CampaignProspectsReplied(result.Failures);
            }

            return Ok();
        }

    }
}
