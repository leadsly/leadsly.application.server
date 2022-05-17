using Leadsly.Api;
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
    [Route("[controller]")]
    public class DeepScanProspectsForRepliesController : ApiControllerBase
    {
        public DeepScanProspectsForRepliesController(ILogger<DeepScanProspectsForRepliesController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<DeepScanProspectsForRepliesController> _logger;

        /// <summary>
        /// Handles list of prospects that have replied, after executing the DeepScanProspectsForReplies phase.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{halId}")]        
        public async Task<IActionResult> ProspectsThatReplied(string halId, ProspectsRepliedRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing DeepScanProspects action for HalId {halId}", halId);

            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessCampaignProspectsRepliedAsync<IOperationResponse>(request, ct);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to process list of prospects coming from DeepScanProspectsForReplies phase.");
                return BadRequest_CampaignProspectsReplied(result.Failures);
            }

            return Ok();
        }

    }
}
